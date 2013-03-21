using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace MarsLander {
  public enum landedT { landed, flying, crashed };

  class LanderBase {
    private double mAcceleration = 2.0;  // but should be varied
    private double mWind;
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

    double mHeight;
    double mXPosition;
    double mYVelocity;
    double mXVelocity;
    landedT mLanded;  // 0- no, 1- yes, 2- crashed
    double mFuel;
    double mBurn;
    double mThrust;

    public LanderBase() {
      Random rand = new Random();
      double wind = 0.2 * (rand.NextDouble() - 0.5);  // random wind 
      double yVelocity = 10.0 * rand.NextDouble();  // random starting velocity
      initialize(yVelocity, wind);
    }

    public LanderBase(double yVelocity, double wind) {
      initialize(yVelocity, wind);
    }

    private void initialize(double yVelocity, double wind) {
      mHeight = 100.0;  // starting height
      mYVelocity = yVelocity;
      mXPosition = 0.0;
      mXVelocity = 0.0;
      mLanded = 0;  // haven't landed yet
      mFuel = 100.0;   // starting fuel
      mAcceleration = 2.0;
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
      control();  // calculate burn and thrust
      if (mFuel < mBurn) {  // if insuficient fuel, use the rest for burn
        mBurn = mFuel;
      }
      mFuel -= Math.Abs(mBurn);  // subtract fuel
      mYVelocity -= mBurn;  // apply burn 
      if (mFuel < mThrust) {  // if insuficient fuel, use the rest for thrust
        mThrust = mFuel;
      }
      mFuel -= Math.Abs(mThrust);  // subtract fuel
      mXVelocity -= mThrust;    // apply thrust

      mHeight -= mYVelocity;  // subtract because moving down
      mXPosition += mXVelocity + mWind;  // wind 
    }

    public void draw() {
    }

    // calculates the burn - vertical adjustments
    // and the thrust - horizontal adjustments
    // both use fuel
    public void control() {
      mBurn = 1.0;
      mThrust = 0;
    }

    public void simulate(bool display) {
      while (!isLanded()) {
        if (display) {
          UpdateTriggeredEventArgs args = new UpdateTriggeredEventArgs();
          args.height = mHeight;
          args.yVelocity = mYVelocity;
          args.xVelocity = mXVelocity;
          args.xPosition = mXPosition;
          args.fuel = mFuel;
          args.status = getStatus();
          OnUpdateTriggered(args);

          Thread.Sleep(SLEEP_TIME_MS);
        }
        update();
      }
    }
  }

  class Program {
    static void Main(string[] args) {
      LanderBase lander = new LanderBase();
      Display display = new Display();
      lander.UpdateTriggered += display.UpdateTriggeredEventHandler_print;
      lander.simulate(display : true);
      Console.Read();
    }
  }
}
