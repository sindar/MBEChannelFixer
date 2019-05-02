namespace ChannelFixer
{
    partial class MainForm
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.CloseTimer = new System.Windows.Forms.Timer(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.RefreshTimer = new System.Windows.Forms.Timer(this.components);
            this.HideTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // CloseTimer
            // 
            this.CloseTimer.Interval = 1000;
            this.CloseTimer.Tick += new System.EventHandler(this.CloseTimer_Tick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(-4, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(385, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Идёт проверка ip-адресов канала драйвера MBE...";
            // 
            // RefreshTimer
            // 
            this.RefreshTimer.Interval = 1000;
            this.RefreshTimer.Tick += new System.EventHandler(this.RefreshTimer_Tick);
            // 
            // HideTimer
            // 
            this.HideTimer.Interval = 1000;
            this.HideTimer.Tick += new System.EventHandler(this.HideTimer_Tick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(379, 84);
            this.Controls.Add(this.label1);
            this.Name = "MainForm";
            this.Text = "ChannelFixer";
            this.Leave += new System.EventHandler(this.MainForm_Leave);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer CloseTimer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Timer RefreshTimer;
        private System.Windows.Forms.Timer HideTimer;
    }
}

