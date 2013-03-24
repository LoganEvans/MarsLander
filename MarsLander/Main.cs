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

    public static double trialTotal(NeuralLander lander) {
      double score = 0.0;
      for (double yVelocity = 0.0; yVelocity <= 10.0; yVelocity += 2.0) {
        for (double wind = -0.1; wind <= 0.1; wind += 0.1) {
          for (double acceleration = 1.0; acceleration <= 3.0; acceleration += 1.0) {
            score += lander.simulate(false, false, yVelocity, wind, acceleration);
          }
        }
      }
      return score;
    }

    public static int countLands(NeuralLander lander) {
      int score = 0;
      for (double yVelocity = 0.0; yVelocity <= 10.0; yVelocity += 2.0) {
        for (double wind = -0.1; wind <= 0.1; wind += 0.1) {
          for (double acceleration = 1.0; acceleration <= 3.0; acceleration += 1.0) {
            if (lander.simulate(false, false, yVelocity, wind, acceleration) == 0.0) {
              score++;
            }
          }
        }
      }
      return score;
    }

    public static NeuralLander localSearch() {
      NeuralLander champ = null;
      NeuralLander chump;
      double champScore = 100.0;
      double chumpScore = 100.0;
      int tries = 10000;
      int attemptsPerTry = 200;
      int attempts;
      int mutatesPerAttemt = 5;
      int mutates;

      while (tries > 0) {
        Console.WriteLine("Tries left: " + tries);
        tries--;
        // TODO remove the parameters
        champ = new NeuralLander();
        champScore = trialTotal(champ);
        //champScore = champ.simulate(false, false, 3.0, 3.0, 3.0);

        attempts = attemptsPerTry;
        while (attempts > 0) {
          //Console.WriteLine("Attempts left: " + attempts);
          attempts--;

          chump = new NeuralLander(champ);

          mutates = mutatesPerAttemt;
          while (mutates > 0) {
            //Console.WriteLine("Mutates left: " + mutates);
            mutates--;
            chump.mutate();
            chumpScore = trialTotal(chump);
            //chumpScore = chump.simulate(false, false, 3.0, 3.0, 3.0);
            if (chumpScore < champScore) {
              champ = chump;
              champScore = chumpScore;
              attempts = attemptsPerTry;
            }

            if (champScore == 0.0) {
              Console.WriteLine("champScore: " + champScore + ", chumpScore: " + chumpScore + ", lands: " + countLands(champ));
              return champ;
            }
          }
        }
        Console.WriteLine("champScore: " + champScore + ", chumpScore: " + chumpScore + ", lands: " + countLands(champ));
      }

      return champ;
    }

    [STAThread]
    static void Main(string[] args) {
      NeuralLander lander = localSearch();

      Display display = new Display();
      lander.UpdateTriggered += UpdateTriggeredEventHandler_print;
      lander.UpdateTriggered += display.UpdateTriggeredEventHandler_paint;

      Thread displayThread = new Thread((ThreadStart)delegate {
            Application.EnableVisualStyles();
            System.Windows.Forms.Application.Run(display);
        });
      displayThread.Start();

      do {
        lander.simulate(true, true, 0.0, 0.0);
        //lander.simulate(true, false, 2.0, 0.1, 2.0);
        //lander.simulate(true, false, 3.0, 3.0, 3.0);
      } while (MessageBox.Show("Show again?", "Restart Prompt", MessageBoxButtons.YesNo) == DialogResult.Yes);
      Console.WriteLine(lander.ToString());
    }
  }
}
