﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsLander {
  class Utilities {
    private static Random mRand = null;

    public static double getGaussian() {
      if (mRand == null) {
        mRand = new Random();
      }
      double rv = 0;
      for (int i = 0; i < 12; i++) {
        rv += mRand.NextDouble();
      }
      rv -= 6;
      rv *= 1.0 / Math.Sqrt(12);
      return rv;
    }
  }

  class NeuralLander : LanderBase {
    private NeuralVector mInputs = null;
    private NeuralConnection mInputsToHidden = null;
    private NeuralVector mHiddenLayer = null;
    private NeuralConnection mHiddenToOutputs = null;
    private NeuralVector mOutputs = null;
    private const int numInputs = 6;
    private const int numOutputs = 2;

    public NeuralLander() : base() {
      Console.WriteLine(" > NeuralLander()");
      mInputs = new NeuralVector(numInputs);
      mInputsToHidden = new NeuralConnection(numInputs, numInputs);
      mHiddenLayer = new NeuralVector(numInputs);
      mHiddenToOutputs = new NeuralConnection(numInputs, numOutputs);
      mOutputs = new NeuralVector(numOutputs);
    }

    public NeuralLander(double yVelocity, double wind) : base(yVelocity, wind) {
      Console.WriteLine(" > NeuralLander(double, double)");
      mInputs = new NeuralVector(numInputs);
      mInputsToHidden = new NeuralConnection(numInputs, numInputs);
      mHiddenLayer = new NeuralVector(numInputs);
      mHiddenToOutputs = new NeuralConnection(numInputs, numOutputs);
      mOutputs = new NeuralVector(numOutputs);
    }

    public NeuralLander(NeuralLander copyFrom) : base(copyFrom) {
      mInputs = copyFrom.mInputs.Clone();
    }

    public void testIt() {
      NeuralVector nv = new NeuralVector(new double[] {1, 2, 3, 4, 5, 6});
      NeuralConnection nl = new NeuralConnection(new double[,] {{1, 10},
                                                                {2, 20},
                                                                {3, 30},
                                                                {4, 40},
                                                                {5, 50},
                                                                {6, 60}});
      
      NeuralVector cap = new NeuralVector(2);
      Console.WriteLine("nv:\n" + nv.ToString());
      Console.WriteLine("nl:\n" + nl.ToString());
      cap.setInputs(nv * nl);
      Console.WriteLine("cap:\n" + cap.ToString());
    }

    public double[] getInputs() {
      double[] retval = new double[numInputs];
      retval[0] = mXPosition;
      retval[1] = mHeight;
      retval[2] = mXVelocity;
      retval[3] = mYVelocity;
      retval[4] = mFuel;
      retval[5] = 1.0;

      return retval;
    }

    public override void control() {
      mInputs.setInputs(getInputs());
      mInputs.setScalarInput();
      mInputs.applyActivation();
      mHiddenLayer.setInputs(mInputs * mInputsToHidden);
      mHiddenLayer.setScalarInput();
      mHiddenLayer.applyActivation();
      mOutputs.setInputs(mHiddenLayer * mHiddenToOutputs);
      mOutputs.applyActivation();

      mBurn = Math.Abs(mOutputs.at(0));
      mThrust = mOutputs.at(1);
    }

    public void mutate() {
    }
  }

  class NeuralVector {
    private double[] mVector;
    private double[] mActivationConsts;
    private static Random mRand = null;

    public NeuralVector(int numInputs) {
      mVector = new double[numInputs];
      mActivationConsts = new double[numInputs];
    }

    public NeuralVector(double[] vector) {
      setInputs(vector);
      mActivationConsts = new double[mVector.Length];
    }

    public NeuralVector Clone() {
      ////////TODO
    }

    public void setInputs(double[] inputs) {
      mVector = inputs;
      if (inputs.Length != mActivationConsts.Length) {
        Console.WriteLine("mismatch: inputs.Length: " + inputs.Length + ", mActivationConsts.Length: " + mActivationConsts.Length);
      }
    }

    public override string ToString() {
      string retval = "";
      for (int i = 0; i < mVector.Length; i++) {
        retval += mVector[i] + "|" + mActivationConsts[i] + "\n";
      }
      return retval;
    }

    public double at(int col) {
      return mVector[col];
    }

    public void mutateActivationConsts() {
      if (mRand == null) {
        mRand = new Random();
      }
      int selected = mRand.Next(getNumMutatable());
      mActivationConsts[selected] += Utilities.getGaussian();
    }

    public int getNumMutatable() {
      return mActivationConsts.Length;
    }

    public int getCols() {
      return mVector.Length;
    }

    public void applyActivation() {
      for (int i = 0; i < mVector.Length; i++) {
        mVector[i] = 1.0 / (1.0 + Math.Exp(-1 * mVector[i] * mActivationConsts[i]));
      }
    }

    public void setScalarInput() {
      mVector[mVector.Length - 1] = 1.0;
    }
  }

  class NeuralConnection {
    private double[,] mWeights;
    private Random mRand = null;

    public NeuralConnection(int rows, int cols) {
      mWeights = new double[rows, cols];
    }

    public NeuralConnection(double[,] weights) {
      mWeights = weights;
    }

    public int getRows() {
      return mWeights.GetLength(0);
    }

    public int getCols() {
      return mWeights.GetLength(1);
    }

    public static double[] operator *(NeuralVector vector, NeuralConnection matrix) {
      Console.WriteLine(" > operator *");
      double[] retval = new double[matrix.getCols()];

      Console.WriteLine("(op) vector: 1x" + vector.getCols() + ", matrix: " + matrix.getRows() + "x" + matrix.getCols() + " ret: 1x" + retval.Length);

      for (int finalCol = 0; finalCol < matrix.getCols(); finalCol++) {
        for (int offset = 0; offset < matrix.getRows(); offset++) {
          Console.WriteLine("(op) finalCol: " + finalCol + ", offset: " + offset + ", vector.getCols(): " + vector.getCols());
          Console.WriteLine("(op) [" + finalCol + "] " + "vect: " + vector.at(offset) + ", matr: " + matrix.at(offset, finalCol));
          //retval[finalCol] += vector.at(offset) * matrix.at(offset, finalCol);
        }
      }

      for (int i = 0; i < retval.Length; i++) {
        Console.WriteLine("(op)??? " + retval[i]);
      }

      Console.WriteLine(" < operator *() -- retval.Length: " + retval.Length);
      return retval;
    }

    public double at(int row, int col) {
      return mWeights[row, col];
    }

    public override string ToString() {
      string retval = "";
      for (int row_dex = 0; row_dex < mWeights.GetLength(0); row_dex++) {
        for (int col_dex = 0; col_dex < mWeights.GetLength(1); col_dex++) {
          retval += mWeights[row_dex, col_dex].ToString() + ", ";
        }
        retval += "\n";
      }
      return retval;
    }

    public void mutateWeights() {
      if (mRand == null) {
        mRand = new Random();
      }
      int selectedRow = mRand.Next(mWeights.GetLength(0));
      int selectedCol = mRand.Next(mWeights.GetLength(1));
      mWeights[selectedRow, selectedCol] += Utilities.getGaussian();
    }

    public int getNumMutatable() {
      return mWeights.GetLength(0) * mWeights.GetLength(1);
    }
  }
}