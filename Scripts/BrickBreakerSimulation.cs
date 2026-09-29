namespace Game.Scripts;

public enum RoundState
{
  Ready,
  Playing,
  Won,
  Lost
}

// Kinematic rules on the horizontal plane of the 3D table. All distances are metres.
public sealed class BrickBreakerSimulation
{
  public const float HalfWidth = 9.0f;
  public const float BackWall = 10.0f;
  public const float Drain = -11.0f;

  public const float PaddleY = -8.2f;
  public const float PaddleHalfWidth = 1.65f;
  public const float PaddleHalfDepth = 0.35f;
  public const float PaddleSpeed = 16.0f;

  public const float BallRadius = 0.28f;
  public const float InitialBallSpeed = 11.0f;
  public const float MaximumBallSpeed = 18.0f;
  public const float FixedStep = 1.0f / 120.0f;
  public const int BrickCount = 24;

  public RoundState State = RoundState.Ready;
  public bool Paused;
  public int Lives;
  public int Score;

  public float PaddleX;
  public float BallX;
  public float BallY;
  public float VelocityX;
  public float VelocityY;
  public float BallSpeed;
  public bool[] Bricks = new bool[BrickCount];

  // Scratch values for the earliest collision in the current fixed step.
  private float _hitTime;
  private float _normalX;
  private float _normalY;
  private int _hit;

  public BrickBreakerSimulation()
  {
    Reset();
  }

  public void Reset()
  {
    State = RoundState.Ready;
    Paused = false;
    Lives = 3;
    Score = 0;

    PaddleX = 0;
    BallSpeed = InitialBallSpeed;
    for (int i = 0; i < BrickCount; ++i)
      Bricks[i] = true;

    DockBall();
  }

  public void DockBall()
  {
    BallX = PaddleX;
    BallY = PaddleY + PaddleHalfDepth + BallRadius + 0.04f;
    VelocityX = 0;
    VelocityY = 0;
  }

  public void Launch()
  {
    if (State != RoundState.Ready || Paused)
      return;

    State = RoundState.Playing;
    VelocityX = 0.23f;
    VelocityY = 1.0f;
    NormalizeVelocity();
  }

  public int RemainingBricks()
  {
    int count = 0;
    for (int i = 0; i < BrickCount; ++i)
      if (Bricks[i])
        ++count;

    return count;
  }

  private void NormalizeVelocity()
  {
    float factor = BallSpeed / MathF.Sqrt(VelocityX * VelocityX + VelocityY * VelocityY);
    VelocityX *= factor;
    VelocityY *= factor;
  }

  private void ConsiderHit(float time, float normalX, float normalY, int kind)
  {
    if (time >= 0 && time <= _hitTime)
    {
      _hitTime = time;
      _normalX = normalX;
      _normalY = normalY;
      _hit = kind;
    }
  }

  // Sweep against expanded box bounds so fast balls cannot tunnel through bricks.
  private void SweepBrick(int index)
  {
    float enter = -float.MaxValue;
    float leave = _hitTime;
    float normalX = 0;
    float normalY = 0;

    for (int axis = 0; axis < 2; ++axis)
    {
      float center = axis == 0 ? -7.0f + (index % 8) * 2.0f : 3.0f + (index / 8) * 1.6f;
      float halfSize = axis == 0 ? 0.86f : 0.43f;
      float low = center - halfSize - BallRadius;
      float high = center + halfSize + BallRadius;
      float origin = axis == 0 ? BallX : BallY;
      float velocity = axis == 0 ? VelocityX : VelocityY;

      if (MathF.Abs(velocity) < 0.00001f)
      {
        if (origin < low || origin > high)
          return;

        continue;
      }

      float nearTime = (low - origin) / velocity;
      float farTime = (high - origin) / velocity;
      float sign = velocity > 0 ? -1.0f : 1.0f;
      if (nearTime > farTime)
      {
        float swap = nearTime;
        nearTime = farTime;
        farTime = swap;
      }

      if (nearTime > enter)
      {
        enter = nearTime;
        normalX = axis == 0 ? sign : 0;
        normalY = axis == 1 ? sign : 0;
      }

      leave = MathF.Min(leave, farTime);
      if (enter > leave)
        return;
    }

    if (enter < -0.00001f || enter > _hitTime)
      return;

    ConsiderHit(MathF.Max(0.0f, enter), normalX, normalY, index);
  }

  public void Step(float input)
  {
    if (Paused || State == RoundState.Won || State == RoundState.Lost)
      return;

    PaddleX = Math.Clamp(PaddleX + Math.Clamp(input, -1.0f, 1.0f) * PaddleSpeed * FixedStep,
      -HalfWidth + PaddleHalfWidth, HalfWidth - PaddleHalfWidth);

    if (State == RoundState.Ready)
    {
      DockBall();
      return;
    }

    float remaining = FixedStep;
    for (int bounce = 0; bounce < 8 && remaining > 0.000001f; ++bounce)
    {
      _hitTime = remaining;
      _normalX = 0;
      _normalY = 0;
      _hit = -1; // -1: none, -2: wall, -3: paddle, -4: drain; otherwise brick index.

      if (VelocityX < 0)
        ConsiderHit((-HalfWidth + BallRadius - BallX) / VelocityX, 1, 0, -2);

      if (VelocityX > 0)
        ConsiderHit((HalfWidth - BallRadius - BallX) / VelocityX, -1, 0, -2);

      if (VelocityY > 0)
        ConsiderHit((BackWall - BallRadius - BallY) / VelocityY, 0, -1, -2);

      if (VelocityY < 0)
      {
        ConsiderHit((Drain - BallY) / VelocityY, 0, 1, -4);

        // Catch only the paddle's top face, matching the C++ rules.
        float time = (PaddleY + PaddleHalfDepth + BallRadius - BallY) / VelocityY;
        float x = BallX + VelocityX * time;
        if (MathF.Abs(x - PaddleX) <= PaddleHalfWidth + BallRadius)
          ConsiderHit(time, 0, 1, -3);
      }

      for (int i = 0; i < BrickCount; ++i)
        if (Bricks[i])
          SweepBrick(i);

      BallX += VelocityX * _hitTime;
      BallY += VelocityY * _hitTime;
      remaining -= _hitTime;

      if (_hit == -1)
        break;

      if (_hit == -4)
      {
        --Lives;
        State = Lives == 0 ? RoundState.Lost : RoundState.Ready;
        BallSpeed = InitialBallSpeed;

        DockBall();
        return;
      }

      if (_hit >= 0)
      {
        Bricks[_hit] = false;
        Score += 100;
        BallSpeed = MathF.Min(MaximumBallSpeed, BallSpeed + 0.22f);

        if (RemainingBricks() == 0)
        {
          State = RoundState.Won;
          VelocityX = 0;
          VelocityY = 0;
          return;
        }
      }

      if (_hit == -3)
      {
        float offset = Math.Clamp((BallX - PaddleX) / PaddleHalfWidth, -1.0f, 1.0f);
        float horizontal = offset * 0.85f;
        VelocityX = horizontal * BallSpeed;
        VelocityY = MathF.Sqrt(1.0f - horizontal * horizontal) * BallSpeed;
      }
      else
      {
        float dot = VelocityX * _normalX + VelocityY * _normalY;
        VelocityX -= _normalX * (2.0f * dot);
        VelocityY -= _normalY * (2.0f * dot);
        NormalizeVelocity();
      }

      BallX += _normalX * 0.001f;
      BallY += _normalY * 0.001f;
    }
  }
}
