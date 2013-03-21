using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

// Add Reference for System.Drawing
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Add Reference for System.Windows.Forms
using System.Windows.Forms;

namespace MarsLander {
  public class UpdateTriggeredEventArgs : EventArgs {
    public double height {get; set;}
    public double yVelocity {get; set;}
    public double xVelocity {get; set;}
    public double xPosition {get; set;}
    public double fuel {get; set;}
    public landedT status {get; set;}
  }

  public partial class Display : Form {
    public void UpdateTriggeredEventHandler_print(object sender, UpdateTriggeredEventArgs e) {
      Console.WriteLine("Height: " + e.height + " Y-Velocity: " + e.yVelocity +
                        " Position: " + e.xPosition + " X-Velocity: " + e.xVelocity + " Fuel: " + e.fuel);
    }
  }
}
