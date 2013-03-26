using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace MarsLander {
  public enum landedT { landed, flying, crashed };

  public class UpdateTriggeredEventArgs : EventArgs {
    public double height {get; set;}
    public double yVelocity {get; set;}
    public double xVelocity {get; set;}
    public double xPosition {get; set;}
    public double fuel {get; set;}
    public double wind { get; set; }
    public double acceleration { get; set; }
    public landedT status {get; set;}
  }

  public class LanderBase {
    protected double mAcceleration = 2.0;  // but should be varied
    protected double mWind;
    private const double MAX_SAFE_LANDING_SPEED = 4.0;
    private const double MIN_SAFE_X = -0.2;
    private const double MAX_SAFE_X = 0.2;
    private const int SLEEP_TIME_MS = 200;

    public event EventHandler<UpdateTriggeredEventArgs> UpdateTriggered;

    protected virtual void OnUpdateTriggered(UpdateTriggeredEventArgs e) {
      EventHandler<UpdateTriggeredEventArgs> handler = UpdateTriggered;
      if (handler != null) {
        handler(this, e);
      }
    }

    protected double mHeight;
    protected double mXPosition;
    protected double mYVelocity;
    protected double mXVelocity;
    protected double mFuel;

    public LanderBase() {
    }

    public LanderBase(LanderBase copyFrom) {
      initialize(copyFrom.mYVelocity, copyFrom.mWind, copyFrom.mAcceleration);
    }

    private void initialize(double yVelocity, double wind, double acceleration) {
      mHeight = 100.0;  // starting height
      mYVelocity = yVelocity;
      mXPosition = 0.0;
      mXVelocity = 0.0;
      mFuel = 100.0;   // starting fuel
      mAcceleration = acceleration;
      mWind = wind;
    }

    public landedT getStatus() {
      if (mHeight > 0) { // haven't landed yet
        return landedT.flying;
      } else if (mYVelocity > MAX_SAFE_LANDING_SPEED || mXPosition < MIN_SAFE_X || mXPosition > MAX_SAFE_X) {
        return landedT.crashed;
      } else {
        return landedT.landed;
      }
    }

    public bool isLanded() {
      return (getStatus() == landedT.flying) ? false : true;
    }

    // update the lander's altitude 
    public void update() {
      mYVelocity += mAcceleration;  // apply acceleration
      Tuple<double, double> controlVals = control();  // calculate burn and thrust
      double burn = controlVals.Item1;
      double thrust = controlVals.Item2;

      if (mFuel < Math.Abs(burn)) {  // if insuficient fuel, use the rest for burn
        burn = mFuel;
      }
      mFuel -= Math.Abs(burn);  // subtract fuel
      mYVelocity -= burn;  // apply burn 
      if (mFuel < Math.Abs(thrust)) {  // if insuficient fuel, use the rest for thrust
        thrust = mFuel;
      }
      mFuel -= Math.Abs(thrust);  // subtract fuel
      mXVelocity -= thrust;    // apply thrust

      mHeight -= mYVelocity;  // subtract because moving down
      mXPosition += mXVelocity + mWind;  // wind 
    }

    // calculates the burn - vertical adjustments
    // and the thrust - horizontal adjustments
    // both use fuel
    public virtual Tuple<double, double> control() {
      double burn = 1.0;
      double thrust = 0.0;
      return Tuple.Create(burn, thrust);
    }

    public double? simulate(bool display, bool randomize, double yVelocity = 0.0, double wind = 0.0, double acceleration = 1.0) {
      if (randomize) {
        Random rand = new Random();
        initialize(rand.NextDouble() * 10.0, (rand.NextDouble() - 0.5) * 0.2, rand.NextDouble() * 2.0 + 1.0);
      } else {
        initialize(yVelocity, wind, acceleration);
      }

      do {
        update();

        if (display) {
          UpdateTriggeredEventArgs args = new UpdateTriggeredEventArgs();
          args.height = mHeight;
          args.yVelocity = mYVelocity;
          args.xVelocity = mXVelocity;
          args.xPosition = mXPosition;
          args.fuel = mFuel;
          args.status = getStatus();
          args.wind = mWind;
          args.acceleration = mAcceleration;
          OnUpdateTriggered(args);

          Thread.Sleep(SLEEP_TIME_MS);
        }

      } while (!isLanded());

      if (display) {
        Console.WriteLine("Status: " + getStatus() + " | " + getDistance());
      }

      if (getStatus() == landedT.landed) {
        return null;
      } else {
        return getDistance();
      }
    }

    public double getDistance() {
      double score = 0.0;

      if (mYVelocity > MAX_SAFE_LANDING_SPEED) {
        score += mYVelocity - MAX_SAFE_LANDING_SPEED;
      }

      if (Math.Abs(mXPosition) > MAX_SAFE_X) {
        score += Math.Abs(mXPosition) - MAX_SAFE_X;
      }

      return score;
    }
  }
}
