using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MarsLander {
  public partial class Display : Form {
    private UpdateTriggeredEventArgs mDisplayArgs;

    public Display() {
      InitializeComponent();
    }

    private void Display_Load(object sender, EventArgs e) {
    }


    public void UpdateTriggeredEventHandler_paint(object sender, UpdateTriggeredEventArgs args) {
      mDisplayArgs = args;
      Invalidate();  // Triggers OnPaint
    }

    private void OnPaint(object sender, PaintEventArgs e) {
      Graphics g = e.Graphics;
      int pen_width = 2;
      int width = this.Size.Width;
      int deltaX = width / 100;
      int height = this.Size.Height;
      int deltaY = height / 100;

      Pen landerPen = new Pen(Color.Black, pen_width);

      g.Draw

      // Draw rows.
      for (int h_dex = 0; h_dex < ROWS; h_dex++) {
        g.DrawLine(p_lines, new Point(PAD_W, h_dex * delta_h + PAD_H),
                            new Point((COLS - 1) * delta_w + PAD_W, h_dex * delta_h + PAD_H));
      }

      // Draw cols
      for (int w_dex = 0; w_dex < COLS; w_dex++) {
        g.DrawLine(p_lines, new Point(w_dex * delta_w + PAD_W, PAD_H),
                            new Point(w_dex * delta_w + PAD_W, (ROWS - 1) * delta_h + PAD_H));
      }
  }
}
