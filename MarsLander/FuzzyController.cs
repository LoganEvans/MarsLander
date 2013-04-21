using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsLander {
  class FuzzyController : LanderBase {
    private FuzzySet mFSetHeight;
    private FuzzySet mFSetYVelocitySafe;
    private FuzzySet mFSetXVelocityLeft;
    private FuzzySet mFSetXVelocityRight;
    private FuzzySet mFSetSafeLeft;

    private FuzzySet mFSetTooFarLeft;
    private FuzzySet mFSetTooFarRight;

    public FuzzyController() {
      // Ground at 0.0, start at 100.0
      mFSetHeight = new FuzzySet(1.0, 75.0, 200.0, 1000.0);
      mFSetYVelocitySafe = new FuzzySet(-50.0, 0.0, MAX_SAFE_LANDING_SPEED, MAX_SAFE_LANDING_SPEED + 3.0);
      mFSetXVelocityLeft = new FuzzySet(-200.0, -200.0, -0.2, 0.0);
      mFSetXVelocityRight = new FuzzySet(0.0, 0.2, 200.0, 200.0);
      mFSetSafeLeft = new FuzzySet(-200.0, -200.0, 0.0, 0.2);
      mFSetTooFarLeft = new FuzzySet(-200.0, -200.0, -0.2, 0.0);
      mFSetTooFarRight = new FuzzySet(0.0, 0.2, 200, 200);
    }

    public override Tuple<double, double> control() {
      return Tuple.Create(GetBurn(), GetThrust());
    }

    private double GetBurn() {
      double scale = 10.0;
      double isLow = FuzzySet.FuzzyNot(mFSetHeight.GetMembership(mHeight));
      double isTooFast = FuzzySet.FuzzyNot(mFSetYVelocitySafe.GetMembership(mYVelocity));
      double isUnsafeFall = FuzzySet.FuzzyAnd(isLow, isTooFast);
      return scale * isUnsafeFall;
    }

    // Positive outputs move left, negative outputs move right
    private double GetThrust() {
      double scale = 0.3;
      double thrustLeft;
      double thrustRight;

      double isTooFarLeft = mFSetTooFarLeft.GetMembership(mXPosition);
      double isTooFarRight = mFSetTooFarRight.GetMembership(mXPosition);

      double isMovingLeft = mFSetXVelocityLeft.GetMembership(mXVelocity);
      double isMovingRight = mFSetXVelocityRight.GetMembership(mXVelocity);

      thrustLeft = FuzzySet.FuzzyOr(isTooFarRight, isMovingRight);
      thrustRight = FuzzySet.FuzzyOr(isTooFarLeft, isMovingLeft);

      double thrust = scale * (thrustLeft - thrustRight);
   // Console.WriteLine("tooFarLeft: " + isTooFarLeft + " | isTooFarRight: " + isTooFarRight +
   //                   " isMovingLeft: " + isMovingLeft + " isMovingRight: " + isMovingRight + Environment.NewLine +
   //                   " | thrustLeft: " + thrustLeft + " | thrustRight: " + thrustRight + " | thrust: " + thrust);
      return thrust;
    }
  }
}
