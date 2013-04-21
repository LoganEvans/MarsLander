using System;
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

  public class NeuralLander : LanderBase {
    private NeuralVector mInputs = null;
    private NeuralConnection[] mHiddenConnections = null;
    private NeuralVector[] mHiddenLayers = null;
    private NeuralConnection mHiddenToOutputs = null;
    private NeuralVector mOutputs = null;
    private readonly int mNumHiddenLayers = 2;
    //private const int numInputs = 8;
    private const int numInputs = 9;
    private const int numOutputs = 2;
    private static Random mRand = null;

    public NeuralLander() : base() {
      mInputs = new NeuralVector(numInputs);

      mHiddenConnections = new NeuralConnection[mNumHiddenLayers];
      mHiddenLayers = new NeuralVector[mNumHiddenLayers];
      for (int i = 0; i < mNumHiddenLayers; i++) {
        mHiddenConnections[i] = new NeuralConnection(numInputs, numInputs);
        mHiddenLayers[i] = new NeuralVector(numInputs);
      }

      mHiddenToOutputs = new NeuralConnection(numInputs, numOutputs);
      mOutputs = new NeuralVector(numOutputs);

      randomize();
    }

    public NeuralLander(NeuralLander copyFrom) : base(copyFrom) {
      mInputs = new NeuralVector(copyFrom.mInputs);

      mHiddenConnections = new NeuralConnection[mNumHiddenLayers];
      mHiddenLayers = new NeuralVector[mNumHiddenLayers];
      for (int i = 0; i < mNumHiddenLayers; i++) {
        mHiddenConnections[i] = new NeuralConnection(copyFrom.mHiddenConnections[i]);
        mHiddenLayers[i] = new NeuralVector(copyFrom.mHiddenLayers[i]);
      }

      mHiddenToOutputs = new NeuralConnection(copyFrom.mHiddenToOutputs);
      mOutputs = new NeuralVector(copyFrom.mOutputs);
    }

    public override string ToString() {
      string retval = "";

      retval += "mInputs:\n" + mInputs.ToString();
      for (int i = 0; i < mNumHiddenLayers; i++) {
        retval += "mHiddenConnections[" + i + "]:\n" + mHiddenConnections[i].ToString();
        retval += "mHiddenLayers[" + i + "]:\n" + mHiddenLayers[i].ToString();
      }

      retval += "mHiddenToOutputs:\n" + mHiddenToOutputs.ToString();
      retval += "mOutputs:\n" + mOutputs.ToString();

      return retval;
    }

    public double[] getInputs() {
      double[] retval = new double[numInputs];
      retval[0] = mXPosition;
      retval[1] = mHeight;
      retval[2] = mXVelocity;
      retval[3] = mYVelocity;
      retval[4] = mFuel;
      retval[5] = mWind;
      retval[6] = mAcceleration;
      retval[7] = Utilities.getGaussian();
      retval[8] = 1.0;
    //retval[0] = mYVelocity;
    //retval[1] = mWind;
    //retval[2] = 1.0;

      return retval;
    }

    public override Tuple<double, double> control() {
      mInputs.setInputs(getInputs());
      mInputs.applyActivation();
      mInputs.setScalarInput();

      mHiddenLayers[0].setInputs(mInputs * mHiddenConnections[0]);
      mHiddenLayers[0].applyActivation();
      mHiddenLayers[0].setScalarInput();

      for (int i = 1; i < mNumHiddenLayers; i++) {
        mHiddenLayers[i].setInputs(mHiddenLayers[i - 1] * mHiddenConnections[i - 1]);
        mHiddenLayers[i].applyActivation();
        mHiddenLayers[i].setScalarInput();
      }

      mOutputs.setInputs(mHiddenLayers[mNumHiddenLayers - 1] * mHiddenToOutputs);
      mOutputs.applyActivation();

      double burn, thrust;
      burn = Math.Abs(mOutputs.at(0)) * 10.0;
      thrust = mOutputs.at(1) * 10.0;

      return Tuple.Create(burn, thrust);
    }

    public void randomize() {
      mInputs.randomize();
      for (int i = 0; i < mNumHiddenLayers; i++) {
        mHiddenLayers[i].randomize();
        mHiddenConnections[i].randomize();
      }

      mHiddenToOutputs.randomize();
      mOutputs.randomize();
    }

    public void jiggle(double factor) {
      for (int i = 0; i < getNumMutatable(); i++) {
        modify(i, Utilities.getGaussian() * factor);
      }
    }

    public void modify(int index, double augment) {
      if (index < mInputs.getNumMutatable()) {
        mInputs.modify(index, augment);
        return;
      } else {
        index -= mInputs.getNumMutatable();
      }

      for (int i = 0; i < mNumHiddenLayers; i++) {
        if (index < mHiddenConnections[i].getNumMutatable()) {
          mHiddenConnections[i].modify(index, augment);
          return;
        } else {
          index -= mHiddenConnections[i].getNumMutatable();
        }

        if (index < mHiddenLayers[i].getNumMutatable()) {
          mHiddenLayers[i].modify(index, augment);
          return;
        } else {
          index -= mHiddenLayers[i].getNumMutatable();
        }
      }

      if (index < mHiddenToOutputs.getNumMutatable()) {
        mHiddenToOutputs.modify(index, augment);
        return;
      } else {
        index -= mHiddenToOutputs.getNumMutatable();
      }

      if (index < mOutputs.getNumMutatable()) {
        mOutputs.modify(index, augment);
        return;
      } else {
        Console.WriteLine("Error on selected...");
        throw new Exception();
      }
    }

    public int getNumMutatable() {
      int numMutatable = 0;
      numMutatable += mInputs.getNumMutatable();
      for (int i = 0; i < mNumHiddenLayers; i++) {
        numMutatable += mHiddenLayers[i].getNumMutatable();
        numMutatable += mHiddenConnections[i].getNumMutatable();
      }

      numMutatable += mHiddenToOutputs.getNumMutatable();
      numMutatable += mOutputs.getNumMutatable();

      if (mRand == null) {
        mRand = new Random();
      }

      return numMutatable;
    }

    public void mutate() {
      if (mRand == null) {
        mRand = new Random();
      }
      modify(mRand.Next(getNumMutatable()), Utilities.getGaussian());
    }
  }

  public class NeuralVector {
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

    public NeuralVector(NeuralVector copyFrom) {
      mVector = new double[copyFrom.mVector.Length];
      mActivationConsts = new double[copyFrom.mActivationConsts.Length];

      for (int i = 0; i < mVector.Length; i++) {
        mVector[i] = copyFrom.mVector[i];
        mActivationConsts[i] = copyFrom.mActivationConsts[i];
      }
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
      for (int i = 0; i < mActivationConsts.Length; i++) {
        mActivationConsts[i] += Utilities.getGaussian();
      }
    }

    public void mutate(int activationConst) {
      mActivationConsts[activationConst] += Utilities.getGaussian();
    }

    public void randomize() {
      for (int i = 0; i < mActivationConsts.Length; i++) {
        mActivationConsts[i] += 5.0 * Utilities.getGaussian();
      }
    }

    public void modify(int index, double augment) {
      mActivationConsts[index] += augment;
    }

    public int getNumMutatable() {
      return mActivationConsts.Length;
    }

    public int getCols() {
      return mVector.Length;
    }

    public void applyActivation() {
      for (int i = 0; i < mVector.Length; i++) {
        mVector[i] = 2.0 * (1.0 / (1.0 + Math.Exp(-1 * mVector[i] * mActivationConsts[i]))) - 1;
      }
    }

    public void setScalarInput() {
      mVector[mVector.Length - 1] = 1.0;
    }
  }

  public class NeuralConnection {
    private double[,] mWeights;
    private Random mRand = null;

    public NeuralConnection(int rows, int cols) {
      mWeights = new double[rows, cols];
    }

    public NeuralConnection(double[,] weights) {
      mWeights = weights;
    }

    public NeuralConnection(NeuralConnection copyFrom) {
      mWeights = new double[copyFrom.getRows(), copyFrom.getCols()];

      for (int row_dex = 0; row_dex < getRows(); row_dex++) {
        for (int col_dex = 0; col_dex < getCols(); col_dex++) {
          mWeights[row_dex, col_dex] = copyFrom.mWeights[row_dex, col_dex];
        }
      }
    }

    public int getRows() {
      return mWeights.GetLength(0);
    }

    public int getCols() {
      return mWeights.GetLength(1);
    }

    public static double[] operator *(NeuralVector vector, NeuralConnection matrix) {
      double[] retval = new double[matrix.getCols()];

      for (int finalCol = 0; finalCol < matrix.getCols(); finalCol++) {
        for (int offset = 0; offset < matrix.getRows(); offset++) {
          retval[finalCol] += vector.at(offset) * matrix.at(offset, finalCol);
        }
      }

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
      for (int row_dex = 0; row_dex < mWeights.GetLength(0); row_dex++) {
        for (int col_dex = 0; col_dex < mWeights.GetLength(1); col_dex++) {
          mWeights[row_dex, col_dex] += Utilities.getGaussian();
        }
      }
    }

    public void mutate(int index) {
      mWeights[index / mWeights.GetLength(1), index % mWeights.GetLength(1)] += Utilities.getGaussian();
    }

    public void randomize() {
      for (int row_dex = 0; row_dex < mWeights.GetLength(0); row_dex++) {
        for (int col_dex = 0; col_dex < mWeights.GetLength(1); col_dex++) {
          mWeights[row_dex, col_dex] += 5.0 * Utilities.getGaussian();
        }
      }
    }

    public void modify(int index, double augment) {
      mWeights[index / mWeights.GetLength(1), index % mWeights.GetLength(1)] += augment;
    }

    public int getNumMutatable() {
      return mWeights.GetLength(0) * mWeights.GetLength(1);
    }
  }
}
