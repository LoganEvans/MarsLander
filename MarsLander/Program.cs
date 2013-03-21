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
    public landedT status {get; set;}
  }

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
          OnUpdateTriggered(args);

          Thread.Sleep(SLEEP_TIME_MS);
        }

      } while (!isLanded());

      Console.WriteLine("Status: " + getStatus());
    }
  }

  class Program {
    public static void UpdateTriggeredEventHandler_print(object sender, UpdateTriggeredEventArgs args) {
      Console.WriteLine("Height: " + args.height + " Y-Velocity: " + args.yVelocity +
                        " Position: " + args.xPosition + " X-Velocity: " + args.xVelocity + " Fuel: " + args.fuel);
    }

    [STAThread]
    static void Main(string[] args) {
      LanderBase lander = new LanderBase();
      Display display = new Display();
      lander.UpdateTriggered += UpdateTriggeredEventHandler_print;
      lander.UpdateTriggered += display.UpdateTriggeredEventHandler_paint;

      Thread displayThread = new Thread((ThreadStart)delegate {
            Application.EnableVisualStyles();
            System.Windows.Forms.Application.Run(display);
        });
      displayThread.Start();
      lander.simulate(display : true);
    }
  }
}
