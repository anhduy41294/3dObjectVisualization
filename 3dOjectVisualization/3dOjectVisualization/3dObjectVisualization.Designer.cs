namespace _3dOjectVisualization
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.grbFeature = new System.Windows.Forms.GroupBox();
            this.chkShift = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.btnImport = new System.Windows.Forms.Button();
            this.grbFeature.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Location = new System.Drawing.Point(13, 13);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(706, 425);
            this.panel1.TabIndex = 0;
            // 
            // grbFeature
            // 
            this.grbFeature.Controls.Add(this.checkBox1);
            this.grbFeature.Controls.Add(this.checkBox2);
            this.grbFeature.Controls.Add(this.chkShift);
            this.grbFeature.Location = new System.Drawing.Point(726, 13);
            this.grbFeature.Name = "grbFeature";
            this.grbFeature.Size = new System.Drawing.Size(184, 150);
            this.grbFeature.TabIndex = 1;
            this.grbFeature.TabStop = false;
            this.grbFeature.Text = "Features Algorithm";
            // 
            // chkShift
            // 
            this.chkShift.AutoSize = true;
            this.chkShift.Location = new System.Drawing.Point(6, 29);
            this.chkShift.Name = "chkShift";
            this.chkShift.Size = new System.Drawing.Size(57, 17);
            this.chkShift.TabIndex = 0;
            this.chkShift.Text = "SHIFT";
            this.chkShift.UseVisualStyleBackColor = true;
            this.chkShift.CheckedChanged += new System.EventHandler(this.chkShift_CheckedChanged);
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(6, 52);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(53, 17);
            this.checkBox2.TabIndex = 0;
            this.checkBox2.Text = "Algo2";
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(6, 75);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(53, 17);
            this.checkBox1.TabIndex = 0;
            this.checkBox1.Text = "Algo3";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // btnImport
            // 
            this.btnImport.Location = new System.Drawing.Point(732, 181);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(75, 23);
            this.btnImport.TabIndex = 2;
            this.btnImport.Text = "Import";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(922, 513);
            this.Controls.Add(this.btnImport);
            this.Controls.Add(this.grbFeature);
            this.Controls.Add(this.panel1);
            this.Name = "Form1";
            this.Text = "3D Object Visualization";
            this.grbFeature.ResumeLayout(false);
            this.grbFeature.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox grbFeature;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox chkShift;
        private System.Windows.Forms.Button btnImport;
    }
}

