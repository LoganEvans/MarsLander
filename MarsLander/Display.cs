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
    private Image mLanderImage;
    private Image mLanderPlatformBlock;

    public Display() {
      InitializeComponent();
      mDisplayArgs = null;
      mLanderImage = MarsLander.Properties.Resources.pinkie_pie_balloons;
      mLanderPlatformBlock = MarsLander.Properties.Resources.cloud;
    }

    private void Display_Load(object sender, EventArgs e) {
    }

    public void UpdateTriggeredEventHandler_paint(object sender, UpdateTriggeredEventArgs args) {
      mDisplayArgs = args;
      Invalidate();  // Triggers OnPaint
    }

    private void OnPaint(object sender, PaintEventArgs e) {
      if (mDisplayArgs == null) {
        return;
      }

      Graphics g = e.Graphics;
      int pen_width = 2;
      int width = this.Size.Width;
      int centerWidth = width / 2;
      int deltaX = width / 100;
      int height = this.Size.Height;
      int deltaY = height / 100;
      int padTop = 10 * deltaY;
      int padBottom = 3 * deltaX;

      Pen landerPen = new Pen(Color.Black, pen_width);

      Rectangle landerRect = new Rectangle();
      landerRect.Width = 10 * deltaX;
      landerRect.Height = padTop;
      //landerRect.Location = new Point(50, deltaY * mDisplayArgs.height - landerRect.Height - padBottom);
      landerRect.Location = new Point((int)mDisplayArgs.xPosition * deltaX + centerWidth - landerRect.Width / 2,
                                      (100 - (int)mDisplayArgs.height) * deltaY - landerRect.Height - padBottom);
      g.DrawImage(mLanderImage, landerRect);

      Rectangle platformRect = new Rectangle();
      platformRect.Width = deltaX;
      platformRect.Height = padBottom;

      for (int i = -2; i < 2; i++) {
        platformRect.Location = new Point(centerWidth + i * deltaX, height - platformRect.Height - padBottom);
        g.DrawImage(mLanderPlatformBlock, platformRect);
      }
    }
  }
}
