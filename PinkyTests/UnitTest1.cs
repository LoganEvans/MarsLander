using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using MarsLander;

namespace PinkieTests {
  [TestClass]
  public class FuzzyTests {
    [TestMethod]
    public void TestGetMembership() {
      FuzzySet uut = new FuzzySet(1, 2, 3, 4);
      double tv;
      tv = uut.GetMembership(0.5);
      Assert.AreEqual(tv, 0.0, 0.00000001);
      tv = uut.GetMembership(1.0);
      Assert.AreEqual(tv, 0.0, 0.00000001);
      tv = uut.GetMembership(1.2);
      Assert.AreEqual(tv, 0.2, 0.00000001);
      tv = uut.GetMembership(1.8);
      Assert.AreEqual(tv, 0.8, 0.00000001);
      tv = uut.GetMembership(2.0);
      Assert.AreEqual(tv, 1.0, 0.00000001);
      tv = uut.GetMembership(2.3);
      Assert.AreEqual(tv, 1.0, 0.00000001);
      tv = uut.GetMembership(2.9);
      Assert.AreEqual(tv, 1.0, 0.00000001);
      tv = uut.GetMembership(3.1);
      Assert.AreEqual(tv, 0.9, 0.00000001);
      tv = uut.GetMembership(3.4);
      Assert.AreEqual(tv, 0.6, 0.00000001);
      tv = uut.GetMembership(3.9);
      Assert.AreEqual(tv, 0.1, 0.00000001);
      tv = uut.GetMembership(4.1);
      Assert.AreEqual(tv, 0.0, 0.00000001);

      uut = new FuzzySet(-1.0, 0.0, 3.0, 5.0);
      tv = uut.GetMembership(-1.1);
      Assert.AreEqual(tv, 0.0, 0.00000001);
      tv = uut.GetMembership(-0.5);
      Assert.AreEqual(tv, 0.5, 0.00000001);
      tv = uut.GetMembership(1.2);
      Assert.AreEqual(tv, 1.0, 0.00000001);
      tv = uut.GetMembership(3.4);
      Assert.AreEqual(tv, 0.8, 0.00000001);
      tv = uut.GetMembership(4.8);
      Assert.AreEqual(tv, 0.1, 0.00000001);
    }
  }
}
