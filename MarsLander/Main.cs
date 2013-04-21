using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MarsLander {
  public class PinkyLander {
    private static StreamWriter sLog;

    public static void UpdateTriggeredEventHandler_print(object sender, UpdateTriggeredEventArgs args) {
      Console.WriteLine("Height: " + args.height + " Y-Velocity: " + args.yVelocity +
                        " Position: " + args.xPosition + " X-Velocity: " + args.xVelocity + " Fuel: " + args.fuel +
                        " Wind: " + args.wind + " Acceleration: " + args.acceleration);
    }

    public static List<Tuple<double, double, double>> simParams = null;

    public static void initializeSimParams() {
      simParams = new List<Tuple<double, double, double>>();

      for (double yVelocity = 0.0; yVelocity <= 10.005; yVelocity += 2.0) {
        for (double wind = 0.0; wind <= 0.205; wind += 0.1) {
          for (double acceleration = 1.0; acceleration <= 3.005; acceleration += 1.0) {
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
      foreach (Tuple<double, double, double> param in simParams) {
        scoreToAdd = lander.simulate(false, false, param.Item1, param.Item2, param.Item3);
        if (scoreToAdd == null) {
          lands += 1;
        } else {
          score += (double)scoreToAdd;
        }
      }
      return Tuple.Create(score, lands);
    }

    public static NeuralLander systematicLocalSearchRestarts(int trials, int indecies) {
      NeuralLander testDummy;
      NeuralLander best = new NeuralLander();
      Tuple<double, int> statsDummy, statsBest, prevStatsDummy, preJiggleStats;
      statsBest = stats(best);

      for (int trial = 0; trial < trials; trial++) {
        testDummy = new NeuralLander();
        for (int i = 0; i < indecies; i++) {
          testDummy = systematicLocalSearchRandomIndex(testDummy, indecies);
        }
        statsDummy = stats(testDummy);
        do {
          preJiggleStats = statsDummy;
          testDummy.jiggle(0.1);
          do {
            prevStatsDummy = stats(testDummy);
            testDummy = systematicLocalSearch(testDummy);
            statsDummy = stats(testDummy);
          } while (prevStatsDummy.Item1 > statsDummy.Item1);
        } while (preJiggleStats.Item1 > statsDummy.Item1);

        if (statsDummy.Item1 < statsBest.Item1) {
          best = testDummy;
          statsBest = statsDummy;
        }

        if (statsBest.Item1 == 0.0) {
          sLog.WriteLine("Found a solution: " + statsBest + " at " + DateTime.Now);
          return best;
        }
        sLog.WriteLine("Time: " + DateTime.Now + ", Round: " + trial + ", best: " + statsBest + ", testDummy: " + statsDummy);
        Console.WriteLine("Round: " + trial + ", best: " + statsBest + ", testDummy: " + statsDummy);
        sLog.Flush();
      }
      return best;
    }

    public static NeuralLander systematicLocalSearchRandomIndex(NeuralLander lander, int indecies) {
      Random rand = new Random();
      for (int i = 0; i < indecies; i++) {
        lander = systematicLocalSearchOnIndex(lander, rand.Next() % lander.getNumMutatable());
      }
      return lander;
    }

    public static NeuralLander systematicLocalSearch(NeuralLander lander) {
      for (int i = lander.getNumMutatable() - 1; i >= 0; i--) {
        lander = systematicLocalSearchOnIndex(lander, i);
        //Console.WriteLine("Stats: " + stats(lander));
      }
      return lander;
    }

    public static NeuralLander systematicLocalSearchOnIndex(NeuralLander lander, int index) {
      double deltaOrig = 1.0;
      double delta = deltaOrig;
      NeuralLander chump;
      Tuple<double, int> statsChump, statsLander;
      statsLander = stats(lander);
      bool modified;

      while (delta > deltaOrig / 16.0) {
        modified = false;
        chump = new NeuralLander(lander);
        chump.modify(index, delta);
        statsChump = stats(chump);

        if (statsChump.Item1 < statsLander.Item1) {
          lander = chump;
          statsLander = statsChump;
          modified = true;
        }

        if (!modified) {
          chump.modify(index, -2 * delta);
          statsChump = stats(chump);

          if (statsChump.Item1 < statsLander.Item1) {
            lander = chump;
            statsLander = statsChump;
            modified = true;
          }
        }

        if (!modified) {
          delta /= 2.0;
        }
      }

      return lander;
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
        sLog.WriteLine("Score for " + param + ": " + stats(retval) + " ... " +
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
        //sLog.WriteLine("try: " + i);
        nextTry = new NeuralLander();
        nextTry = localSearch(nextTry, attempts, mutates);
        statistics = stats(nextTry);

        if (statistics.Item1 < bestStats.Item1) {
          best = nextTry;
          bestStats = statistics;
          //sLog.WriteLine("Best so far: " + bestStats);
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
      double champScore;
      double chumpScore;
      double bestScore = stats(best).Item1;
      int attempts;
      int mutates;
      champScore = stats(champ).Item1;

      attempts = attemptsPerTry;
      while (attempts > 0) {
        attempts--;
        chump = new NeuralLander(champ);
        mutates = mutatesPerAttempt;
        while (mutates > 0) {
          mutates--;
          chump.mutate();
          chumpScore = stats(chump).Item1;
          if (chumpScore < champScore) {
            champ = chump;
            champScore = chumpScore;
            attempts = attemptsPerTry;

            if (champScore < bestScore) {
              best = champ;
              bestScore = champScore;
              sLog.WriteLine("(attempt) bestStats: " + stats(best));
            }
          }

          if (champScore == 0.0) {
            sLog.WriteLine("champScore: " + champScore + ", chumpScore: " + chumpScore + ", lands: " + stats(champ).Item2);
            return champ;
          }
        }
      }

      return best;
    }

    public static void finalTest(LanderBase lander) {
      int trials = 0;
      int crashes = 0;
      int crashesPerAcc;
      int trialsPerAcc;
      double? result;
      for (double acceleration = 1.0; acceleration <= 3.05; acceleration += 0.1) {
        crashesPerAcc = 0;
        trialsPerAcc = 0;
        for (double yVelocity = 0.0; yVelocity <= 10.05; yVelocity += 0.5) {
          for (double wind = -0.19; wind <= 0.205; wind += 0.05) {
            trialsPerAcc++;
            trials++;
            result = lander.simulate(false, false, yVelocity, wind, acceleration);
            if (result != null) {
              sLog.WriteLine("yVelocity: " + yVelocity + ", wind: " + wind + ", acceleration: " + acceleration + ", result: " + result);
              crashes++;
              crashesPerAcc++;
            }
          }
        }

        sLog.WriteLine("At acceleration: " + acceleration + " Trials: " + trialsPerAcc + " Crashes: " + crashesPerAcc);
      }
      sLog.WriteLine("Trials: " + trials + " Crashes: " + crashes);
      Console.WriteLine("Trials: " + trials + " Crashes: " + crashes);
      Console.ReadKey();
    }

    [STAThread]
    static void Main(string[] args) {
      sLog = File.AppendText(@"C:\Users\Logan\Desktop\pinkie_lander_output.txt");
      sLog.WriteLine();
      sLog.WriteLine(DateTime.Now);
      FuzzyController lander = new FuzzyController();

      Display display = new Display();
      lander.UpdateTriggered += UpdateTriggeredEventHandler_print;
      lander.UpdateTriggered += display.UpdateTriggeredEventHandler_paint;
      Thread displayThread = new Thread((ThreadStart)delegate {
        Application.EnableVisualStyles();
        System.Windows.Forms.Application.Run(display);
      });

      initializeSimParams();
      finalTest(lander);

      /*
      displayThread.Start();
      var trial = Tuple.Create(0.0, -0.2, 1.0);
      do {
        //lander.simulate(true, true);
        lander.simulate(true, false, trial.Item1, trial.Item2, trial.Item3);
      } while (MessageBox.Show("Show again?", "Restart Prompt", MessageBoxButtons.YesNo) == DialogResult.Yes);
      */

      //NeuralLander lander = new NeuralLander();
      //Tuple<double, double, double> trial = Tuple.Create(10.0, -0.2, 3.0);
      //Tuple<double, double, double> trial2 = Tuple.Create(1.0, 0.2, 3.0);
      //Tuple<double, int> statsBest, statsPrev, statsLander;

      //NeuralLander best = new NeuralLander(lander);
      //statsBest = stats(best);
      //int round = 0;

      //while (statsBest.Item1 != 0.0) {
      //  lander = new NeuralLander();
      //  statsLander = stats(lander);
      //  statsPrev = statsLander;
      //  do {
      //    statsPrev = statsLander;
      //    lander.jiggle(0.5);
      //    lander = systematicLocalSearch(lander);
      //    statsLander = stats(lander);
      //    if (statsLander.Item1 < statsBest.Item1) {
      //      best = new NeuralLander(lander);
      //      statsBest = statsLander;
      //    }
      //  } while (statsLander.Item1 < statsPrev.Item1);
      //  Console.WriteLine("Round: " + round + " best: " + statsBest + ", current: " + statsPrev);
      //  round++;
      //}

      ////lander = systematicLocalSearchRestarts(1000, 500);
      //finalTest(lander);
      ////lander = searchForSolution(lander, trial, 100);

      ////sLog.WriteLine(lander.ToString());

      //Display display = new Display();
      //lander.UpdateTriggered += UpdateTriggeredEventHandler_print;
      //lander.UpdateTriggered += display.UpdateTriggeredEventHandler_paint;

      //Thread displayThread = new Thread((ThreadStart)delegate {
      //      Application.EnableVisualStyles();
      //      System.Windows.Forms.Application.Run(display);
      //  });

      //displayThread.Start();

      //do {
      //  //lander.simulate(true, true);
      //  lander.simulate(true, false, trial.Item1, trial.Item2, trial.Item3);
      //} while (MessageBox.Show("Show again?", "Restart Prompt", MessageBoxButtons.YesNo) == DialogResult.Yes);

      ////sLog.Close();
    }
  }
}
