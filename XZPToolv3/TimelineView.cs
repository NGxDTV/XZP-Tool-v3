using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace XZPToolv3
{
    public class TimelineView : Control
    {
        private TimelineData data;
        private readonly HScrollBar hScroll;
        private float zoom = 1.0f;

        public TimelineView()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            BackColor = SystemColors.Window;
            ForeColor = SystemColors.ControlText;
            Font = new Font("Segoe UI", 8.25f, FontStyle.Regular);

            hScroll = new HScrollBar();
            hScroll.Dock = DockStyle.Bottom;
            hScroll.Minimum = 0;
            hScroll.SmallChange = 20;
            hScroll.LargeChange = 100;
            hScroll.ValueChanged += (s, e) => Invalidate();
            Controls.Add(hScroll);
        }

        public void SetData(TimelineData newData)
        {
            data = newData;
            UpdateScroll();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle bounds = ClientRectangle;
            bounds.Height -= hScroll.Height;
            using (SolidBrush bg = new SolidBrush(BackColor))
                g.FillRectangle(bg, bounds);

            if (data == null)
            {
                DrawMessage(g, "No selection.");
                return;
            }

            if (!string.IsNullOrEmpty(data.Title))
            {
                using (Font titleFont = new Font(Font, FontStyle.Bold))
                using (Brush titleBrush = new SolidBrush(ForeColor))
                    g.DrawString(data.Title, titleFont, titleBrush, new PointF(8, 6));
            }

            if (!string.IsNullOrEmpty(data.Message))
            {
                DrawMessage(g, data.Message);
                return;
            }

            int left = 110;
            int right = 12;
            int top = 28;
            int rowHeight = 24;
            int viewWidth = Math.Max(1, bounds.Width - left - right);
            float virtualWidth = viewWidth * zoom;
            float scrollOffset = hScroll.Value;

            uint maxTime = data.MaxTime == 0 ? 1 : data.MaxTime;

            DrawTimeRuler(g, left, top - 10, viewWidth, maxTime, scrollOffset, virtualWidth);

            int row = 0;
            if (data.NamedFrames.Count > 0)
            {
                DrawRowLine(g, left, top + row * rowHeight, viewWidth, "Named");
                DrawMarkers(g, left, top + row * rowHeight, viewWidth, maxTime, data.NamedFrames, scrollOffset, virtualWidth);
                row++;
            }

            foreach (TimelineTrack track in data.Tracks)
            {
                int y = top + row * rowHeight;
                DrawRowLine(g, left, y, viewWidth, track.Id);
                DrawKeyframes(g, left, y, viewWidth, maxTime, track.Keyframes, scrollOffset, virtualWidth);
                row++;
            }
        }

        private void DrawMessage(Graphics g, string message)
        {
            using (Brush textBrush = new SolidBrush(Color.Gray))
                g.DrawString(message, Font, textBrush, new PointF(6, 22));
        }

        private void DrawRowLine(Graphics g, int left, int y, int width, string label)
        {
            using (Pen linePen = new Pen(Color.FromArgb(180, 180, 180), 1))
                g.DrawLine(linePen, left, y, left + width, y);

            using (Brush textBrush = new SolidBrush(ForeColor))
                g.DrawString(label ?? "Timeline", Font, textBrush, new PointF(8, y - 10));
        }

        private void DrawMarkers(Graphics g, int left, int y, int width, uint maxTime, List<NamedFrame> frames, float scrollOffset, float virtualWidth)
        {
            foreach (NamedFrame frame in frames)
            {
                float t = Math.Min(maxTime, frame.Time) / (float)maxTime;
                float x = left + t * virtualWidth - scrollOffset;
                if (x < left - 10 || x > left + width + 10)
                    continue;
                PointF[] tri =
                {
                    new PointF(x, y - 6),
                    new PointF(x - 5, y - 16),
                    new PointF(x + 5, y - 16)
                };
                using (Brush b = new SolidBrush(Color.FromArgb(60, 120, 200)))
                    g.FillPolygon(b, tri);
            }
        }

        private void DrawKeyframes(Graphics g, int left, int y, int width, uint maxTime, List<uint> frames, float scrollOffset, float virtualWidth)
        {
            foreach (uint frame in frames)
            {
                float t = Math.Min(maxTime, frame) / (float)maxTime;
                float x = left + t * virtualWidth - scrollOffset;
                if (x < left - 10 || x > left + width + 10)
                    continue;
                RectangleF dot = new RectangleF(x - 3, y - 3, 6, 6);
                using (Brush b = new SolidBrush(Color.FromArgb(220, 80, 60)))
                    g.FillEllipse(b, dot);
                using (Pen p = new Pen(Color.FromArgb(120, 60, 60), 1))
                    g.DrawEllipse(p, dot);
            }
        }

        private void DrawTimeRuler(Graphics g, int left, int y, int width, uint maxTime, float scrollOffset, float virtualWidth)
        {
            using (Pen tickPen = new Pen(Color.FromArgb(210, 210, 210), 1))
            using (Brush textBrush = new SolidBrush(Color.Gray))
            {
                int ticks = 5;
                for (int i = 0; i < ticks; i++)
                {
                    float t = i / (float)(ticks - 1);
                    float x = left + t * width;
                    float valueT = (scrollOffset + (x - left)) / virtualWidth;
                    valueT = Math.Max(0f, Math.Min(1f, valueT));
                    g.DrawLine(tickPen, x, y, x, y + 6);
                    uint labelVal = (uint)(maxTime * valueT);
                    string label = labelVal.ToString();
                    SizeF size = g.MeasureString(label, Font);
                    g.DrawString(label, Font, textBrush, x - size.Width / 2, y + 8);
                }
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if ((ModifierKeys & Keys.Control) == Keys.Control)
            {
                float delta = e.Delta > 0 ? 0.1f : -0.1f;
                zoom = Math.Max(0.5f, Math.Min(5.0f, zoom + delta));
                UpdateScroll();
                Invalidate();
                return;
            }

            int newVal = hScroll.Value - e.Delta;
            newVal = Math.Max(hScroll.Minimum, Math.Min(hScroll.Maximum - hScroll.LargeChange + 1, newVal));
            hScroll.Value = newVal;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            UpdateScroll();
        }

        private void UpdateScroll()
        {
            Rectangle bounds = ClientRectangle;
            int viewWidth = Math.Max(1, bounds.Width - 110 - 12);
            float virtualWidth = viewWidth * zoom;

            int max = (int)Math.Max(0, virtualWidth - viewWidth);
            hScroll.LargeChange = Math.Max(10, viewWidth / 2);
            hScroll.Maximum = Math.Max(0, max + hScroll.LargeChange - 1);
            hScroll.SmallChange = 20;
            hScroll.Value = Math.Max(hScroll.Minimum, Math.Min(hScroll.Value, Math.Max(0, max)));
            hScroll.Enabled = max > 0;
        }
    }

    public class TimelineData
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public uint MaxTime { get; set; }
        public List<TimelineTrack> Tracks { get; set; } = new List<TimelineTrack>();
        public List<NamedFrame> NamedFrames { get; set; } = new List<NamedFrame>();
    }

    public class TimelineTrack
    {
        public string Id { get; set; }
        public List<uint> Keyframes { get; set; } = new List<uint>();
    }

    public class NamedFrame
    {
        public string Name { get; set; }
        public uint Time { get; set; }
        public string Command { get; set; }
    }
}
