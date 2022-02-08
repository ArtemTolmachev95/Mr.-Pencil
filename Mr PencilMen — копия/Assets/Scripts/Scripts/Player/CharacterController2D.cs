using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterController2D : MonoBehaviour
{
	[SerializeField] private float m_JumpForce = 400f;                          // Количество добавленной силы, когда игрок прыгает.
	[Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;  // Насколько нужно сгладить движение
	[SerializeField] private bool m_AirControl = false;                         // Может ли игрок управлять во время прыжка;
	[SerializeField] private LayerMask m_WhatIsGround;                          // Маска, определяющая, что является основой для персонажа
	[SerializeField] private Transform m_GroundCheck;                           // Обозначение позиции, где можно проверить, заземлен ли игрок.
	[SerializeField] private Transform m_WallCheck;                             // Положение, которое контролирует, касается ли персонаж стены
	


	const float k_GroundedRadius = .2f;                  // Радиус окружности перекрытия для определения того, заземлен ли
	private bool m_Grounded;                             // Независимо от того, заземлен игрок или нет.
	private Rigidbody2D m_Rigidbody2D;
	private bool m_FacingRight = true;                   // Для определения того, в какую сторону в данный момент смотрит игрок.
	private Vector3 velocity = Vector3.zero;
	private float limitFallSpeed = 25f;                  // Ограничить скорость падения

	public bool canDoubleJump = true; //Если игрок может совершить двойной прыжок
	[SerializeField] private float m_DashForce = 25f;
	private bool canDash = true;
	private bool isDashing = false; //Если игрок делает двойной прыжок
	private bool m_IsWall = false; //Если перед игроком есть стена
	private bool isWallSliding = false; //Если игрок скользит по стене
	private bool oldWallSlidding = false; //Если игрок скользит по стене в предыдущем кадре
	private float prevVelocityX = 0f;
	private bool canCheck = false; //Для проверки, скользит ли игрок по стене

	public float life = 10f; //Жизнь игрока
	public bool invincible = false; //Если игрок умерет
	private bool canMove = true; //Если игрок может двигаться
	public int numOfHearts; // количество сердец
	public Image[] hearts; // массив картинок сердец
	public Sprite fullHeart; // Спрайт полного сердечка
	public Sprite emptyHeart; // Спрайт пустого сердца



	private Animator animator; // Аниматор
	public ParticleSystem particleJumpUp; //Частицы 
	public ParticleSystem particleJumpDown; //Частицы взрыва

	private float jumpWallStartX = 0;
	private float jumpWallDistX = 0; //Расстояние между игроком и стеной
	private bool limitVelOnWallJump = false; //Для ограничения расстояния прыжка со стены при низком кадре в секунду

	



	[Header("Events")]
	[Space]

	public UnityEvent OnFallEvent;
	public UnityEvent OnLandEvent;

	[System.Serializable]
	public class BoolEvent : UnityEvent<bool> { }



	private void Awake()
	{
		m_Rigidbody2D = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();

		if (OnFallEvent == null)
			OnFallEvent = new UnityEvent();

		if (OnLandEvent == null)
			OnLandEvent = new UnityEvent();
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.tag == "Life" && life >= 10)
		{

			Destroy(other.gameObject);


		}
		if (other.gameObject.tag == "Life" && life < 10)
		{
			life = life + 5;
			if (life > 10)
			{
				life = 10;

			}
			PlayerPrefs.SetFloat("life", life);
			Destroy(other.gameObject);
			Debug.Log("+5 к здоровью");

		}

		if (other.gameObject.tag == "life + 1" && life < 10)
		{
			life = life + 1;
			if (life > 10)
			{
				life = 10;

			}
			PlayerPrefs.SetFloat("life", life);
			Destroy(other.gameObject);
			Debug.Log("+1 к здоровью");
			

		}
		if (other.gameObject.tag == "life + 1" && life >= 10)
		{

			Destroy(other.gameObject);
			PlayerPrefs.SetFloat("life", life);


		}

		if (other.gameObject.tag == "Saw")
		{
			life = life * 0;
			animator.SetBool("IsDead", true);
			SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
		}


	}

	

	private void FixedUpdate()
	{

		

			if (life > numOfHearts)
			{
				life = numOfHearts;

			}


		for (int i = 0; i < hearts.Length; i++)
			{
				if (i < Mathf.RoundToInt(life))
				{
					hearts[i].sprite = fullHeart;
				}
				else
				{
					hearts[i].sprite = emptyHeart;
				}
				if (i < numOfHearts)
				{
					hearts[i].enabled = true;
				}
				else
				{
					hearts[i].enabled = false;
				}

			
		}

		





		bool wasGrounded = m_Grounded;
		m_Grounded = false;
		// Игрок заземляется, если круговой бросок в позицию проверки земли попадает во что-либо, обозначенное как земля
		// Вместо этого это можно сделать с помощью слоев, но образцы ресурсов не будут перезаписывать настройки вашего проекта.
		Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i].gameObject != gameObject)
				m_Grounded = true;
				if (!wasGrounded )
				{
					OnLandEvent.Invoke();
					if (!m_IsWall && !isDashing) 
						particleJumpDown.Play();
					canDoubleJump = true;
					if (m_Rigidbody2D.velocity.y < 0f)
						limitVelOnWallJump = false;
				}
		}

		m_IsWall = false;

		if (!m_Grounded)
		{
			OnFallEvent.Invoke();
			Collider2D[] collidersWall = Physics2D.OverlapCircleAll(m_WallCheck.position, k_GroundedRadius, m_WhatIsGround);
			for (int i = 0; i < collidersWall.Length; i++)
			{
				if (collidersWall[i].gameObject != null)
				{
					isDashing = false;
					m_IsWall = true;
				}
			}
			prevVelocityX = m_Rigidbody2D.velocity.x;
		}

		if (limitVelOnWallJump)
		{
			if (m_Rigidbody2D.velocity.y < -0.5f)
				limitVelOnWallJump = false;
			jumpWallDistX = (jumpWallStartX - transform.position.x) * transform.localScale.x;
			if (jumpWallDistX < -0.5f && jumpWallDistX > -1f) 
			{
				canMove = true;
			}
			else if (jumpWallDistX < -1f && jumpWallDistX >= -2f) 
			{
				canMove = true;
				m_Rigidbody2D.velocity = new Vector2(10f * transform.localScale.x, m_Rigidbody2D.velocity.y);
			}
			else if (jumpWallDistX < -2f) 
			{
				limitVelOnWallJump = false;
				m_Rigidbody2D.velocity = new Vector2(0, m_Rigidbody2D.velocity.y);
			}
			else if (jumpWallDistX > 0) 
			{
				limitVelOnWallJump = false;
				m_Rigidbody2D.velocity = new Vector2(0, m_Rigidbody2D.velocity.y);
			}
		}



	}


	public void Move(float move, bool jump, bool dash)
	{
		if (canMove) {
			if (dash && canDash && !isWallSliding)
			{
				//m_Rigidbody2D.AddForce(new Vector2(transform.localScale.x * m_DashForce, 0f));
				StartCoroutine(DashCooldown());
			}
			// Если вы сидите на корточках, проверьте, может ли персонаж встать
			if (isDashing)
			{
				m_Rigidbody2D.velocity = new Vector2(transform.localScale.x * m_DashForce, 0);
			}
			//управляйте плеером только в том случае, если заземлен или включен AirControl
			else if (m_Grounded || m_AirControl)
			{
				if (m_Rigidbody2D.velocity.y < -limitFallSpeed)
					m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, -limitFallSpeed);
				// Перемещайте персонажа, определяя целевую скорость
				Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
				// А затем сглаживаем его и применяем к персонажу
				m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref velocity, m_MovementSmoothing);

				// Если ввод перемещает игрока вправо, а игрок смотрит влево...
				if (move > 0 && !m_FacingRight && !isWallSliding)
				{
					// ... переверните игрока
					Flip();
				}
				// В противном случае, если вход перемещает игрока влево, а игрок смотрит вправо...
				else if (move < 0 && m_FacingRight && !isWallSliding)
				{
					// ... переверните игрока
					Flip();
				}
			}
			// Если игрок должен прыгнуть...
			if (m_Grounded && jump)
			{
				// Добавьте игроку вертикальную силу.
				animator.SetBool("IsJumping", true);
				animator.SetBool("JumpUp", true);
				m_Grounded = false;
				m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
				canDoubleJump = true;
				particleJumpDown.Play();
				particleJumpUp.Play();
			}
			else if (!m_Grounded && jump && canDoubleJump && !isWallSliding)
			{
				canDoubleJump = false;
				m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, 0);
				m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce / 1.2f));
				animator.SetBool("IsDoubleJumping", true);
			}

			else if (m_IsWall && !m_Grounded)
			{
				if (!oldWallSlidding && m_Rigidbody2D.velocity.y < 0 || isDashing)
				{
					isWallSliding = true;
					m_WallCheck.localPosition = new Vector3(-m_WallCheck.localPosition.x, m_WallCheck.localPosition.y, 0);
					Flip();
					StartCoroutine(WaitToCheck(0.1f));
					canDoubleJump = true;
					animator.SetBool("IsWallSliding", true);
				}
				isDashing = false;

				if (isWallSliding)
				{
					if (move * transform.localScale.x > 0.1f)
					{
						StartCoroutine(WaitToEndSliding());
					}
					else 
					{
						oldWallSlidding = true;
						m_Rigidbody2D.velocity = new Vector2(-transform.localScale.x * 2, -5);
					}
				}

				if (jump && isWallSliding)
				{
					animator.SetBool("IsJumping", true);
					animator.SetBool("JumpUp", true); 
					m_Rigidbody2D.velocity = new Vector2(0f, 0f);
					m_Rigidbody2D.AddForce(new Vector2(transform.localScale.x * m_JumpForce *1.2f, m_JumpForce));
					jumpWallStartX = transform.position.x;
					limitVelOnWallJump = true;
					canDoubleJump = true;
					isWallSliding = false;
					animator.SetBool("IsWallSliding", false);
					oldWallSlidding = false;
					m_WallCheck.localPosition = new Vector3(Mathf.Abs(m_WallCheck.localPosition.x), m_WallCheck.localPosition.y, 0);
					canMove = false;
				}
				else if (dash && canDash)
				{
					isWallSliding = false;
					animator.SetBool("IsWallSliding", false);
					oldWallSlidding = false;
					m_WallCheck.localPosition = new Vector3(Mathf.Abs(m_WallCheck.localPosition.x), m_WallCheck.localPosition.y, 0);
					canDoubleJump = true;
					StartCoroutine(DashCooldown());
				}
			}
			else if (isWallSliding && !m_IsWall && canCheck) 
			{
				isWallSliding = false;
				animator.SetBool("IsWallSliding", false);
				oldWallSlidding = false;
				m_WallCheck.localPosition = new Vector3(Mathf.Abs(m_WallCheck.localPosition.x), m_WallCheck.localPosition.y, 0);
				canDoubleJump = true;
			}
		}
	}


	private void Flip()
	{
		// Switch the way the player is labelled as facing.
		m_FacingRight = !m_FacingRight;

		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}

	public void ApplyDamage(float damage, Vector3 position) 
	{
		if (!invincible)
		{
			animator.SetBool("Hit", true);
			life -= damage;
			Vector2 damageDir = Vector3.Normalize(transform.position - position) * 40f ;
			m_Rigidbody2D.velocity = Vector2.zero;
			m_Rigidbody2D.AddForce(damageDir * 10);
			if (life <= 0)
			{
				StartCoroutine(WaitToDead());
			}
			else
			{
				StartCoroutine(Stun(0.25f));
				StartCoroutine(MakeInvincible(1f));
			}
		}
	}

	IEnumerator DashCooldown()
	{
		animator.SetBool("IsDashing", true);
		isDashing = true;
		canDash = false;
		yield return new WaitForSeconds(0.1f);
		isDashing = false;
		yield return new WaitForSeconds(0.5f);
		canDash = true;
	}

	IEnumerator Stun(float time) 
	{
		canMove = false;
		yield return new WaitForSeconds(time);
		canMove = true;
	}
	IEnumerator MakeInvincible(float time) 
	{
		invincible = true;
		yield return new WaitForSeconds(time);
		invincible = false;
	}
	IEnumerator WaitToMove(float time)
	{
		canMove = false;
		yield return new WaitForSeconds(time);
		canMove = true;
	}

	IEnumerator WaitToCheck(float time)
	{
		canCheck = false;
		yield return new WaitForSeconds(time);
		canCheck = true;
	}

	IEnumerator WaitToEndSliding()
	{
		yield return new WaitForSeconds(0.1f);
		canDoubleJump = true;
		isWallSliding = false;
		animator.SetBool("IsWallSliding", false);
		oldWallSlidding = false;
		m_WallCheck.localPosition = new Vector3(Mathf.Abs(m_WallCheck.localPosition.x), m_WallCheck.localPosition.y, 0);
	}

	IEnumerator WaitToDead()
	{
		animator.SetBool("IsDead", true);
		canMove = false;
		invincible = true;
		GetComponent<Attack>().enabled = false;
		yield return new WaitForSeconds(0.4f);
		m_Rigidbody2D.velocity = new Vector2(0, m_Rigidbody2D.velocity.y);
		yield return new WaitForSeconds(1.1f);
		SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
	}

	

}
