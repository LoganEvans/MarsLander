using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MarsLander {
  public class PinkyLander {
    public static void UpdateTriggeredEventHandler_print(object sender, UpdateTriggeredEventArgs args) {
      Console.WriteLine("Height: " + args.height + " Y-Velocity: " + args.yVelocity +
                        " Position: " + args.xPosition + " X-Velocity: " + args.xVelocity + " Fuel: " + args.fuel);
    }

    public static List<Tuple<double, double, double>> simParams = null;

    public static void initializeSimParams() {
      simParams = new List<Tuple<double, double, double>>();

      for (double yVelocity = 0.0; yVelocity <= 10.0; yVelocity += 2.0) {
        for (double wind = 0.0; wind <= 0.2; wind += 0.1) {
          for (double acceleration = 1.0; acceleration <= 3.0; acceleration += 1.0) {
            simParams.Add(Tuple.Create(yVelocity, wind, acceleration));
          }
        }
      }
      simParams.Sort((a, b) => a.Item3.CompareTo(b.Item3));
    }

    public static Tuple<double, int> stats(NeuralLander lander, List<Tuple<double, double, double>> sims = null) {
      if (sims == null) {
        sims = simParams;
      }
      double score = 0.0;
      double? scoreToAdd;
      int lands = 0;
      //int order = 1;
      foreach (Tuple<double, double, double> param in simParams) {
        scoreToAdd = lander.simulate(false, false, param.Item1, param.Item2, param.Item3);
        if (scoreToAdd == null) {
          lands += 1;
        } else {
          score += (double)scoreToAdd; //  *(order);
          //order++;
          //score += 9.26;  // If all 54 are added, this will add 500 to the score.
        }
      }
      return Tuple.Create(score, lands);
    }

    public static NeuralLander searchForSolution(NeuralLander lander, Tuple<double, double, double> param, int tries = 10) {
      NeuralLander champ = new NeuralLander(lander);
      NeuralLander chump;
      NeuralLander best;
      int attemptsPerTry = 200, attempts, mutatesPerAttempt = 30, mutates;
      double? champScore, chumpScore, bestScore;
      best = lander;
      bestScore = best.simulate(false, false, param.Item1, param.Item2, param.Item3);

      while (tries > 0) {
        tries--;
        attempts = attemptsPerTry;
        champScore = champ.simulate(false, false, param.Item1, param.Item2, param.Item3);
        champ = new NeuralLander(lander);

        while (attempts > 0) {
          attempts--;

          chump = new NeuralLander(champ);
          mutates = mutatesPerAttempt;
          while (mutates > 0) {
            mutates--;

            chump.mutate();

            chumpScore = chump.simulate(false, false, param.Item1, param.Item2, param.Item3);
            if (chumpScore == null) {
              return chump;
            }

            if (chumpScore < champScore) {
              champ = chump;
              champScore = chumpScore;
              attempts = attemptsPerTry;

              if (champScore < bestScore) {
                best = champ;
                bestScore = champScore;
              }
              break;
            }
          }
        }
      }

      return best;
    }

    public static NeuralLander searchForAll(NeuralLander lander, int tries = 3, List<Tuple<double, double, double>> sims = null) {
      if (sims == null) {
        sims = simParams;
      }
      NeuralLander retval = new NeuralLander(lander);
      foreach (Tuple<double, double, double> param in sims) {
        retval = localSearch(retval);
        retval = searchForSolution(retval, param, tries);
        Console.WriteLine("Score for " + param + ": " + stats(retval) + " ... " +
                          retval.simulate(false, false, param.Item1, param.Item2, param.Item3));
      }
      return retval;
    }

    public static NeuralLander localSearchRestarts(int tries, int attempts, int mutates) {
      NeuralLander nextTry = null;
      NeuralLander best = new NeuralLander();
      Tuple<double, int> statistics = null, bestStats;
      bestStats = stats(best);

      for (int i = 0; i < tries; i++) {
        //Console.WriteLine("try: " + i);
        nextTry = new NeuralLander();
        nextTry = localSearch(nextTry, attempts, mutates);
        statistics = stats(nextTry);

        if (statistics.Item1 < bestStats.Item1) {
          best = nextTry;
          bestStats = statistics;
          //Console.WriteLine("Best so far: " + bestStats);
        }

        if (bestStats.Item1 == 0.0) {
          return best;
        }

      }
      return best;
    }

    public static NeuralLander localSearch(NeuralLander seed, int attemptsPerTry = 50, int mutatesPerAttempt = 30) {
      NeuralLander champ = seed;
      NeuralLander chump;
      NeuralLander best = champ;
      initializeSimParams();
      double champScore = 100.0;
      double chumpScore = 100.0;
      double bestScore = stats(best).Item1;
      int attempts;
      int mutates;
      champScore = stats(champ).Item1;

      attempts = attemptsPerTry;
      while (attempts > 0) {
        attempts--;
        if (attempts % 20 == 0) {
          //Console.WriteLine("Attempts left: " + attempts);
        }

        chump = new NeuralLander(champ);

        mutates = mutatesPerAttempt;
        while (mutates > 0) {
          //Console.WriteLine("Mutates left: " + mutates);
          mutates--;
          chump.mutate();
          chumpScore = stats(chump).Item1;
          //chumpScore = chump.simulate(false, false, 3.0, 3.0, 3.0);
          if (chumpScore < champScore) {
            champ = chump;
            champScore = chumpScore;
            attempts = attemptsPerTry;

            if (champScore < bestScore) {
              best = champ;
              bestScore = champScore;
              //Console.WriteLine("(attempt) bestStats: " + stats(best));
            }
          }

          if (champScore == 0.0) {
            Console.WriteLine("champScore: " + champScore + ", chumpScore: " + chumpScore + ", lands: " + stats(champ).Item2);
            return champ;
          }
        }
      }

      return best;
    }

    [STAThread]
    static void Main(string[] args) {
      //NeuralLander lander = localSearch();
      initializeSimParams();
      NeuralLander lander = new NeuralLander();
      Tuple<double, double, double> trial = Tuple.Create(10.0, -0.2, 3.0);
      Tuple<double, double, double> trial2 = Tuple.Create(1.0, 0.2, 3.0);

      //lander = searchForSolution(lander, trial);
      //lander = localSearchRestarts(10, 200, 30);
      lander = searchForAll(lander, 1);

      /*
      List<Tuple<double, double, double>> tryit = new List<Tuple<double, double, double>>();
      tryit.Add(trial);
      tryit.Add(trial2);
      lander = searchForAll(lander, 1, tryit);

      lander = searchForSolution(lander, trial);
      lander = searchForSolution(lander, trial2);
      lander = searchForAll(lander, 1);
      Console.WriteLine("Round two!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
      lander = searchForAll(lander, 1);
      Console.WriteLine("Round three!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
      lander = localSearch(lander);
      */

      Display display = new Display();
      lander.UpdateTriggered += UpdateTriggeredEventHandler_print;
      lander.UpdateTriggered += display.UpdateTriggeredEventHandler_paint;

      Thread displayThread = new Thread((ThreadStart)delegate {
            Application.EnableVisualStyles();
            System.Windows.Forms.Application.Run(display);
        });
      displayThread.Start();

      do {
        lander.simulate(true, false, trial.Item1, trial.Item2, trial.Item3);
        //lander.simulate(true, false, 2.0, 0.2, 2.0);
        //lander.simulate(true, false, 2.0, 0.1, 2.0);
        //lander.simulate(true, false, 3.0, 3.0, 3.0);
      } while (MessageBox.Show("Show again?", "Restart Prompt", MessageBoxButtons.YesNo) == DialogResult.Yes);
      Console.WriteLine(lander.ToString());
    }
  }
}
