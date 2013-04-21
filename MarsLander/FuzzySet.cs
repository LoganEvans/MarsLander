using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsLander {
  public class FuzzySet {
    double mLeftZero;
    double mLeftOne;
    double mRightOne;
    double mRightZero;

    public FuzzySet(Tuple<double, double, double, double> boundaries) : this(boundaries.Item1, boundaries.Item2, boundaries.Item3, boundaries.Item4) {}

    public FuzzySet(double leftZero, double leftOne, double rightOne, double rightZero) {
      mLeftZero = leftZero;
      mLeftOne = leftOne;
      mRightOne = rightOne;
      mRightZero = rightZero;
    }

    public double GetMembership(double val) {
      if (val <= mLeftZero || mRightZero <= val) {
        return 0.0;
      } else if (val <= mLeftOne) {
        return (val - mLeftZero) / (mLeftOne - mLeftZero);
      } else if (mRightOne <= val) {
        return 1.0 - (val - mRightOne) / (mRightZero - mRightOne);
      } else {
        return 1.0;
      }
    }

    public static double FuzzyAnd(double val1, double val2) {
      return Math.Min(val1, val2);
    }

    public static double FuzzyOr(double val1, double val2) {
      return Math.Max(val1, val2);
    }

    public static double FuzzyNot(double val) {
      return 1 - val;
    }
  }
}
