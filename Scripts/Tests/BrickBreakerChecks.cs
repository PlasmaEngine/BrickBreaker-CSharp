namespace Game.Scripts;

// Runs the same scenarios as the C++ template against the actual script simulation.
public sealed class BrickBreakerChecks
{
  public int Failures; 
  public int Checks;

  private void Check(bool condition)
  {
    ++Checks;
    if (!condition)
      ++Failures;
  }

  public bool Run()
  {
    Failures = 0;
    Checks = 0;
    BrickBreakerSimulation game = new BrickBreakerSimulation();

    Check(game.RemainingBricks() == 24 && game.Lives == 3 && game.Score == 0);

    for (int i = 0; i < 600; ++i)
      game.Step(1);

    Check(MathF.Abs(game.PaddleX - 7.35f) < 0.001f && game.BallX == game.PaddleX && game.State == RoundState.Ready);

    game.Paused = true;
    float pausedX = game.BallX;
    game.Launch();
    game.Step(-1);
    Check(game.BallX == pausedX && game.State == RoundState.Ready);

    game.Paused = false;
    game.Launch();
    Check(game.State == RoundState.Playing && game.VelocityY > 0);

    game.Reset();
    game.Launch();
    game.BallX = 8.71f;
    game.BallY = 0;
    game.VelocityX = 10;
    game.VelocityY = 5;
    game.Step(0);
    Check(game.VelocityX < 0 && game.BallX <= 8.72f);

    for (int contact = -1; contact <= 1; ++contact)
    {
      game.Reset();
      game.Launch();
      game.BallX = contact;
      game.BallY = -7.55f;
      game.VelocityX = 0;
      game.VelocityY = -11;
      game.Step(0);

      float speed = MathF.Sqrt(game.VelocityX * game.VelocityX + game.VelocityY * game.VelocityY);
      Check(game.VelocityY > 0 && MathF.Abs(speed - game.BallSpeed) < 0.001f &&
        (contact == 0 ? MathF.Abs(game.VelocityX) < 0.001f : contact * game.VelocityX > 0));
    }

    game.Reset();
    game.Launch();
    game.BallX = -7;
    game.BallY = 1.8f;
    game.VelocityX = 0;
    game.VelocityY = 180;
    game.Step(0);
    Check(game.Score == 100 && game.RemainingBricks() == 23 && game.VelocityY < 0);

    for (int i = 0; i < 8; ++i)
      game.Step(0);
    Check(game.Score == 100);

    game.Reset();
    for (int i = 1; i < 24; ++i)
      game.Bricks[i] = false;

    game.Launch();
    game.BallX = -7;
    game.BallY = 2.2f;
    game.VelocityX = 0;
    game.VelocityY = 18;
    game.Step(0);
    Check(game.State == RoundState.Won && game.RemainingBricks() == 0);

    float wonX = game.BallX;
    float wonY = game.BallY;
    game.Step(1);
    Check(game.BallX == wonX && game.BallY == wonY);

    game.Reset();
    for (int remainingLives = 2; remainingLives >= 0; --remainingLives)
    {
      game.Launch();
      game.BallX = 7;
      game.BallY = -10.99f;
      game.VelocityX = 0;
      game.VelocityY = -11;
      game.Step(0);
      Check(game.Lives == remainingLives && game.State == (remainingLives == 0 ? RoundState.Lost : RoundState.Ready));
    }

    game.Launch();
    game.Step(0);
    Check(game.Lives == 0 && game.State == RoundState.Lost);

    game.Paused = true;
    game.Reset();
    Check(game.RemainingBricks() == 24 && game.Lives == 3 && game.Score == 0 && !game.Paused &&
      game.BallSpeed == 11.0f && game.State == RoundState.Ready);

    // A vertical sweep in the gap between bricks must not report a brick hit.
    game.Launch();
    game.BallX = 0;
    game.BallY = 1.0f;
    game.VelocityX = 0;
    game.VelocityY = 100;
    game.Step(0);
    Check(game.Score == 0);

    game.Reset();
    for (int tick = 0; tick < 120 * 240 && game.State != RoundState.Won && game.State != RoundState.Lost; ++tick)
    {
      game.Launch();
      float target = game.BallX + MathF.Sin(tick * 0.37f * (MathF.PI / 180.0f)) * 1.1f;
      float input = Math.Clamp((target - game.PaddleX) / (16.0f * (1.0f / 120.0f)), -1.0f, 1.0f);
      game.Step(input);
    }

    Check(game.State == RoundState.Won && game.Score == 2400 && game.RemainingBricks() == 0);
    return Failures == 0;
  }
}
