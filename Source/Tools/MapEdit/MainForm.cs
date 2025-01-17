﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Apoc3D.Config;
using Apoc3D.Vfs;
using Code2015.BalanceSystem;
using Code2015.EngineEx;

namespace MapEdit
{
    public partial class MainForm : Form
    {
        Image currentImage;

        List<MapObject> objectList = new List<MapObject>();
        Dictionary<string, MapCity> cityTable = new Dictionary<string, MapCity>();


        MapObject selectedObject;

        ObjectType filter;
        bool drawString;
        bool drawRadius;
      

        bool isDraging;

        List<Image> bgImages = new List<Image>();

        Graphics g = null;
        Image cityImage, resWoodImage, resOilImage, soundImage, sceneImage;
        Image imageEx;
        Pen pen;
        Brush brush;
        Font font;

        SolidBrush red, green, blue, yellow, purple;
        
        MapObject SelectedObject
        {
            get { return selectedObject; }
            set
            {
                if (selectedObject != value)
                {
                    panel1.Visible = false;
                    panel2.Visible = false;
                    panel3.Visible = false;
                    panel4.Visible = false;
                    
                    if (selectedObject != null)
                    {
                        selectedObject.IsSelected = false;
                    }

                    selectedObject = value;

                    if (selectedObject != null)
                    {
                        selectedObject.IsSelected = true;    
                        switch (selectedObject.Type)
                        {
                            case ObjectType.City:

                                MapCity city = (MapCity)selectedObject.Tag;
                                textBox1.Text = city.Name;
                                numericUpDown2.Value = city.FarmCount;

                                numericUpDown1.Value = (decimal)city.ProblemHunger;
                                numericUpDown3.Value = (decimal)city.ProblemEducation;
                                numericUpDown4.Value = (decimal)city.ProblemGender;
                                numericUpDown5.Value = (decimal)city.ProblemChild;
                                numericUpDown6.Value = (decimal)city.ProblemMaternal;
                                numericUpDown7.Value = (decimal)city.ProblemDisease;
                                numericUpDown8.Value = (decimal)city.ProblemEnvironment;
                                numericUpDown14.Value = (decimal)city.StartUp;
                                switch (city.Size)
                                {
                                    case UrbanSize.Small:
                                        radioButton5.Checked = true;
                                        break;
                                    case UrbanSize.Medium:
                                        radioButton6.Checked = true;
                                        break;
                                    case UrbanSize.Large:
                                        radioButton7.Checked = true;
                                        break;
                                }

                                string r = "";
                                for (int i = 0; i < city.LinkableCity.Length; i++)
                                {
                                    r += city.LinkableCity[i];
                                    if (i != city.LinkableCity.Length - 1)
                                        r += ", ";
                                } textBox4.Text = r;

                                panel1.Dock = DockStyle.Fill;
                                panel1.Visible = true;
                                break;
                            case ObjectType.ResWood:
                            case ObjectType.ResOil:
                                MapResource oil = (MapResource)selectedObject.Tag;

                                numericUpDown9.Value = (decimal)oil.Amount;
                                numericUpDown10.Value = (decimal)oil.Radius;
                                switch (oil.Type)
                                {
                                    case NaturalResourceType.Wood:
                                        radioButton1.Checked = true;
                                        break;
                                    case NaturalResourceType.Petro:
                                        radioButton2.Checked = true;
                                        break;
                                }
                                panel2.Dock = DockStyle.Fill;
                                panel2.Visible = true;
                                break;
                            case ObjectType.Scene:
                                MapSceneObject so = (MapSceneObject)selectedObject.Tag;

                                checkBox1.Checked = so.IsForest;

                                numericUpDown12.Value = (decimal)so.Amount;
                                numericUpDown13.Value = (decimal)so.Radius;

                                textBox3.Text = so.Model;

                                panel4.Dock = DockStyle.Fill;
                                panel4.Visible = true;
                                break;
                            case ObjectType.Sound:
                                MapSoundObject sndObj = (MapSoundObject)selectedObject.Tag;

                                comboBox2.Text = sndObj.SFXName;
                                numericUpDown11.Value = (decimal)sndObj.Radius;

                                panel3.Dock = DockStyle.Fill;
                                panel3.Visible = true;
                                break;
                        }
                    }
                }
            }
        }
        public MainForm()
        {
            InitializeComponent();
            g = pictureBox1.CreateGraphics();
            ConfigurationManager.Initialize();
            ConfigurationManager.Instance.Register(new GameConfigurationFormat());
        }


        private void MainForm_Load(object sender, EventArgs e)
        {
            imageEx = Image.FromFile("ImageEx.png");


            cityImage = Image.FromFile("City.png");
            resWoodImage = Image.FromFile("ResWood.png");
            resOilImage = Image.FromFile("ResOil.png");
            soundImage = Image.FromFile("Sound.png");
            sceneImage = Image.FromFile("Scene.png");

            pen = new Pen(Color.White, 3);
            brush = pen.Brush;
            font = new Font("Arial", 7, FontStyle.Regular);


            red = new SolidBrush(Color.FromArgb(120, Color.Red));
            green = new SolidBrush(Color.FromArgb(120, Color.Yellow));
            blue = new SolidBrush(Color.FromArgb(120, Color.Blue));
            yellow = new SolidBrush(Color.FromArgb(120, Color.Purple));
            purple = new SolidBrush(Color.FromArgb(120, Color.Green));

            Image img = Image.FromFile("map_hcity.png");
            checkedListBox1.Items.Add(img);
            bgImages.Add(img);

            pictureBox1.Paint += DrawAll;

        }

        private void pictureBox1_Resize(object sender, EventArgs e)
        {
            MapObject.MapWidth = pictureBox1.Width;
            MapObject.MapHeight = pictureBox1.Height;
            pictureBox1.Refresh();
        }


        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Image img = Image.FromFile(openFileDialog1.FileName);
                checkedListBox1.Items.Add(img);
                bgImages.Add(img);
            }

            pictureBox1.Refresh();

        }

        void DrawSelection(Graphics g, int x, int y)
        {
            const int AdjX = -MapObject.IconWidth;
            const int AdjY = -MapObject.IconHeight;

            g.DrawLine(pen, new Point(x + AdjX, y + AdjY), new Point(x + AdjX, y - AdjX));
            g.DrawLine(pen, new Point(x + AdjX, y - AdjY), new Point(x - AdjX, y - AdjX));
            g.DrawLine(pen, new Point(x + AdjX, y + AdjY), new Point(x - AdjX, y + AdjX));
            g.DrawLine(pen, new Point(x - AdjX, y + AdjY), new Point(x - AdjX, y - AdjX));

        }
        void DrawLink(Graphics g, MapCity city, int x, int y)
        {

            string[] v = city.LinkableCity;
            if (v != null)
            {

                for (int i = 0; i < v.Length; i++)
                {
                    int cx, cy;

                    MapCity cc = cityTable[v[i]];


                    MapObject.GetMapCoord(Apoc3D.MathLib.MathEx.Degree2Radian(cc.Longitude), Apoc3D.MathLib.MathEx.Degree2Radian(cc.Latitude), out cx, out cy);
                    g.DrawLine(pen, new Point(x, y), new Point(cx, cy));

                }
            }
        }

        private void DrawAll(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (currentImage != null)
            {
                g.DrawImage(currentImage, new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height));
            }
            g.DrawImage(imageEx, 5, 5);
            const int AdjX = -MapObject.IconWidth / 2;
            const int AdjY = -MapObject.IconHeight / 2;


            float yscale = MapObject.MapHeight / 462f;
            float xscale = MapObject.MapWidth / 1188f;

            for (int i = 0; i < objectList.Count; i++)
            {
                MapObject m = objectList[i];

                SolidBrush selectedRB = null;

                if (m.IsSelected)
                {
                    DrawSelection(g, m.X, m.Y);
                }
                switch (m.Type)
                {
                    case ObjectType.City:
                        if ((filter & ObjectType.City) == ObjectType.City)
                        {
                            int x = m.X;
                            int y = m.Y;
                            if (m.IsSelected)
                            {
                                DrawLink(g, (MapCity)m.Tag, x, y);
                            }

                            g.DrawImage(cityImage, x + AdjX, y + AdjY);

                            if (drawString && !string.IsNullOrEmpty(m.StringDisplay))
                            {
                                g.DrawString(m.StringDisplay, font, brush, x + AdjX, y + AdjY);
                            } if (drawRadius && m.Radius > float.Epsilon) selectedRB = red;
                        }

                        break;
                    case ObjectType.ResWood:
                        if ((filter & ObjectType.ResWood) == ObjectType.ResWood)
                        {
                            g.DrawImage(resWoodImage, m.X + AdjX, m.Y + AdjY);

                            if (drawString && !string.IsNullOrEmpty(m.StringDisplay))
                            {
                                g.DrawString(m.StringDisplay, font, brush, m.X + AdjX, m.Y + AdjY);
                            } if (drawRadius && m.Radius > float.Epsilon) selectedRB = green;

                        }
                        break;
                    case ObjectType.ResOil:
                        if ((filter & ObjectType.ResOil) == ObjectType.ResOil)
                        {
                            g.DrawImage(resOilImage, m.X + AdjX, m.Y + AdjY);

                            if (drawString && !string.IsNullOrEmpty(m.StringDisplay))
                            {
                                g.DrawString(m.StringDisplay, font, brush, m.X + AdjX, m.Y + AdjY);
                            } if (drawRadius && m.Radius > float.Epsilon) selectedRB = blue;

                        }
                        break;
                    case ObjectType.Sound:
                        if ((filter & ObjectType.Sound) == ObjectType.Sound)
                        {
                            g.DrawImage(soundImage, m.X + AdjX, m.Y + AdjY);

                            if (drawString && !string.IsNullOrEmpty(m.StringDisplay))
                            {
                                g.DrawString(m.StringDisplay, font, brush, m.X + AdjX, m.Y + AdjY);
                            } if (drawRadius && m.Radius > float.Epsilon) selectedRB = yellow;

                        }
                        break;
                    case ObjectType.Scene:
                        if ((filter & ObjectType.Scene) == ObjectType.Scene)
                        {
                            g.DrawImage(sceneImage, m.X + AdjX, m.Y + AdjY);

                            if (drawString && !string.IsNullOrEmpty(m.StringDisplay))
                            {
                                g.DrawString(m.StringDisplay, font, brush, m.X + AdjX, m.Y + AdjY);
                            } if (drawRadius && m.Radius > float.Epsilon) selectedRB = purple;

                        }
                        break;

                }
                if (selectedRB != null)
                {
                    float r = Apoc3D.MathLib.MathEx.Degree2Radian(m.Radius);
                    r = ((r + Apoc3D.MathLib.MathEx.PIf) / (2 * Apoc3D.MathLib.MathEx.PIf)) * 1188;
                    r -= ((Apoc3D.MathLib.MathEx.PIf) / (2 * Apoc3D.MathLib.MathEx.PIf)) * 1188;

                    float xr = r * xscale;
                    float yr = r * yscale;

                    RectangleF rect = new RectangleF(m.X - xr, m.Y - yr, xr * 2, yr * 2);

                    g.FillEllipse(selectedRB, rect);
                }
            }
        }


        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            SimulationWorld sim = new SimulationWorld();

            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                string dir = folderBrowserDialog1.SelectedPath;

                Configuration config = ConfigurationManager.Instance.CreateInstance(new FileLocation(Path.Combine(dir, "cities.xml")));

                foreach (KeyValuePair<string, ConfigurationSection> s in config)
                {
                    ConfigurationSection sect = s.Value;
                    MapCity city = new MapCity(sim, sect);

                    MapObject obj = new MapObject();
                    obj.Longitude = city.Longitude;
                    obj.Latitude = city.Latitude;
                    obj.Tag = city;
                    obj.Type = ObjectType.City;
                    obj.StringDisplay = city.Name;
                    obj.SectionName = sect.Name;
                    
                    cityTable.Add(sect.Name, city);

                    objectList.Add(obj);
                }

                config = ConfigurationManager.Instance.CreateInstance(new FileLocation(Path.Combine(dir, "sceneObjects.xml")));
                foreach (KeyValuePair<string, ConfigurationSection> s in config)
                {
                    ConfigurationSection sect = s.Value;

                    MapSceneObject sceObj = new MapSceneObject(sect);
                    MapObject obj = new MapObject();
                    obj.Longitude = sect.GetSingle("Longitude");
                    obj.Latitude = sect.GetSingle("Latitude");
                    obj.Type = ObjectType.Scene;
                    obj.Tag = sceObj;
                    obj.StringDisplay = sceObj.Model;
                    obj.Radius = sceObj.Radius;
                    obj.SectionName = sect.Name;
                    objectList.Add(obj);
                }

                config = ConfigurationManager.Instance.CreateInstance(new FileLocation(Path.Combine(dir, "resources.xml")));

                foreach (KeyValuePair<string, ConfigurationSection> s in config)
                {
                    ConfigurationSection sect = s.Value;

                    MapResource res = new MapResource(sim, sect);

                    MapObject obj = new MapObject();

                    obj.Longitude = res.Longitude;
                    obj.Latitude = res.Latitude;
                    obj.Tag = res;
                    if (res.Type == NaturalResourceType.Wood)
                    {
                        obj.Type = ObjectType.ResWood;
                    }
                    else if (res.Type == NaturalResourceType.Petro)
                    {
                        obj.Type = ObjectType.ResOil;
                    }
                    obj.StringDisplay = (obj.Type == ObjectType.ResOil ? "O" : "W") + res.Amount.ToString();
                    obj.Radius = res.Radius;
                    obj.SectionName = sect.Name;
                    objectList.Add(obj);
                }

                config = ConfigurationManager.Instance.CreateInstance(new FileLocation(Path.Combine(dir, "soundObjects.xml")));
                foreach (KeyValuePair<string, ConfigurationSection> s in config)
                {
                    ConfigurationSection sect = s.Value;

                    MapSoundObject sndObj = new MapSoundObject(sect);

                    MapObject obj = new MapObject();
                    obj.Longitude = sect.GetSingle("Longitude");
                    obj.Latitude = sect.GetSingle("Latitude");

                    obj.Type = ObjectType.Sound;

                    obj.Tag = sndObj;
                    obj.StringDisplay = sndObj.SFXName;
                    obj.Radius = sndObj.Radius;
                    obj.SectionName = sect.Name;
                    objectList.Add(obj);
                  
                    
                }


                config = ConfigurationManager.Instance.CreateInstance(new FileLocation(Path.Combine(dir, "soundEffect.xml")));
                foreach (KeyValuePair<string, ConfigurationSection> s in config)
                {
                    comboBox2.Items.Add(s.Key);
                }
            }
            pictureBox1.Refresh();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                #region city info
                StreamWriter sw = new StreamWriter(
                    File.Open(Path.Combine(folderBrowserDialog1.SelectedPath, "cities.xml"), FileMode.OpenOrCreate),
                    Encoding.UTF8);
                sw.BaseStream.SetLength(0);
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
                sw.WriteLine("<cities>");
                for (int i = 0; i < objectList.Count; i++)
                {
                    MapObject obj = objectList[i];
                    if (obj.Type == ObjectType.City)
                    {
                        MapCity city = (MapCity)obj.Tag;

                        sw.Write("    <"); sw.Write(obj.SectionName); sw.WriteLine(@">");

                        sw.Write("        ");
                        sw.Write("<Name>"); sw.Write(city.Name); sw.WriteLine(@"</Name>");

                        sw.Write("        ");
                        sw.Write("<Longitude>"); sw.Write(obj.Longitude); sw.WriteLine("</Longitude>");

                        sw.Write("        ");
                        sw.Write("<Latitude>"); sw.Write(obj.Latitude); sw.WriteLine("</Latitude>");

                        sw.Write("        ");
                        sw.Write("<Size>"); sw.Write(city.Size.ToString()); sw.WriteLine("</Size>");

                        if (city.ProblemChild != City.DefaultProblemWeight)
                        {
                            sw.Write("        ");
                            sw.Write("<Child>"); sw.Write(city.ProblemChild); sw.WriteLine("</Child>");
                        }
                        if (city.ProblemDisease != City.DefaultProblemWeight)
                        {
                            sw.Write("        ");
                            sw.Write("<Disease>"); sw.Write(city.ProblemDisease); sw.WriteLine("</Disease>");
                        }
                        if (city.ProblemEducation != City.DefaultProblemWeight)
                        {
                            sw.Write("        ");
                            sw.Write("<Education>"); sw.Write(city.ProblemEducation); sw.WriteLine("</Education>");
                        }
                        if (city.ProblemEnvironment != City.DefaultProblemWeight)
                        {
                            sw.Write("        ");
                            sw.Write("<Environment>"); sw.Write(city.ProblemEnvironment); sw.WriteLine("</Environment>");
                        }
                        if (city.ProblemGender != City.DefaultProblemWeight)
                        {
                            sw.Write("        ");
                            sw.Write("<Gender>"); sw.Write(city.ProblemGender); sw.WriteLine("</Gender>");
                        }
                        if (city.ProblemHunger != City.DefaultProblemWeight)
                        {
                            sw.Write("        ");
                            sw.Write("<Hunger>"); sw.Write(city.ProblemHunger); sw.WriteLine("</Hunger>");
                        }
                        if (city.ProblemMaternal != City.DefaultProblemWeight)
                        {
                            sw.Write("        ");
                            sw.Write("<Maternal>"); sw.Write(city.ProblemMaternal); sw.WriteLine("</Maternal>");
                        }
                        if (city.StartUp != -1)
                        {
                            sw.Write("        ");
                            sw.Write("<StartUp>"); sw.Write(city.StartUp); sw.WriteLine("</StartUp>");
                        }
                        if (city.FarmCount != 0)
                        {
                            sw.Write("        ");
                            sw.Write("<Farm>"); sw.Write(city.FarmCount); sw.WriteLine("</Farm>");
                        }



                        string[] linkable = city.LinkableCity;
                        if (linkable != null && linkable.Length > 0)
                        {
                            sw.Write("        "); sw.Write("<Linkable>");
                            for (int j = 0; j < linkable.Length; j++)
                            {
                                sw.Write(linkable[j]);
                                if (j != linkable.Length - 1)
                                    sw.Write(", ");
                            }
                        }
                        sw.WriteLine("</Linkable>");

                        sw.Write("    </"); sw.Write(obj.SectionName); sw.WriteLine(@">");
                    }
                }
                sw.WriteLine("</cities>");
                sw.Close();
                #endregion

                #region resource
                sw = new StreamWriter(
                    File.Open(Path.Combine(folderBrowserDialog1.SelectedPath, "resources.xml"), FileMode.OpenOrCreate),
                    Encoding.UTF8);

                sw.BaseStream.SetLength(0);
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
                sw.WriteLine("<Resources>");
                for (int i = 0; i < objectList.Count; i++)
                {
                    MapObject obj = objectList[i];
                    if (obj.Type == ObjectType.ResOil || obj.Type == ObjectType.ResWood)
                    {
                        MapResource res = (MapResource)obj.Tag;

                        sw.Write("    <"); sw.Write(obj.SectionName); sw.WriteLine(@">");

                        sw.Write("        ");
                        sw.Write("<Longitude>"); sw.Write(obj.Longitude); sw.WriteLine("</Longitude>");

                        sw.Write("        ");
                        sw.Write("<Latitude>"); sw.Write(obj.Latitude); sw.WriteLine("</Latitude>");

                        sw.Write("        ");
                        sw.Write("<Type>"); sw.Write(res.Type.ToString()); sw.WriteLine("</Type>");

                        sw.Write("        ");
                        sw.Write("<Amount>"); sw.Write(res.Amount); sw.WriteLine("</Amount>");


                        if (res.Type == NaturalResourceType.Wood)
                        {
                            sw.Write("        ");
                            sw.Write("<Radius>"); sw.Write(res.Radius); sw.WriteLine("</Radius>");
                        }
                        sw.Write("    </"); sw.Write(obj.SectionName); sw.WriteLine(@">");
                    }
                }

                sw.WriteLine("</Resources>");
                sw.Close();

                #endregion

                #region
                sw = new StreamWriter(
                    File.Open(Path.Combine(folderBrowserDialog1.SelectedPath, "sceneObjects.xml"), FileMode.OpenOrCreate),
                    Encoding.UTF8);

                sw.BaseStream.SetLength(0);
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
                sw.WriteLine("<Scenery>");
                for (int i = 0; i < objectList.Count; i++)
                {
                    MapObject obj = objectList[i];
                    if (obj.Type == ObjectType.Scene)
                    {
                        MapSceneObject sceObj = (MapSceneObject)obj.Tag;

                        sw.Write("    <"); sw.Write(obj.SectionName); sw.WriteLine(@">");

                        sw.Write("        ");
                        sw.Write("<Longitude>"); sw.Write(obj.Longitude); sw.WriteLine("</Longitude>");

                        sw.Write("        ");
                        sw.Write("<Latitude>"); sw.Write(obj.Latitude); sw.WriteLine("</Latitude>");

                        sw.Write("        ");
                        sw.Write("<IsForest>"); sw.Write(sceObj.IsForest); sw.WriteLine("</IsForest>");

                        sw.Write("        ");
                        sw.Write("<Radius>"); sw.Write(sceObj.Radius); sw.WriteLine("</Radius>");

                        sw.Write("        ");
                        sw.Write("<Amount>"); sw.Write(sceObj.Amount); sw.WriteLine("</Amount>");

                        sw.Write("        ");
                        sw.Write("<Model>"); sw.Write(sceObj.Model); sw.WriteLine("</Model>");

                        sw.Write("    </"); sw.Write(obj.SectionName); sw.WriteLine(@">");

                    }
                }
                sw.WriteLine("</Scenery>");
                sw.Close();
                #endregion

                #region
                sw = new StreamWriter(
                    File.Open(Path.Combine(folderBrowserDialog1.SelectedPath, "soundObjects.xml"), FileMode.OpenOrCreate),
                    Encoding.UTF8);

                sw.BaseStream.SetLength(0);
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
                sw.WriteLine("<SoundObjects>");
                for (int i = 0; i < objectList.Count; i++)
                {
                    MapObject obj = objectList[i];
                    if (obj.Type == ObjectType.Sound)
                    {
                        MapSoundObject sndObj = (MapSoundObject)obj.Tag;

                        sw.Write("    <"); sw.Write(obj.SectionName); sw.WriteLine(@">");

                        sw.Write("        ");
                        sw.Write("<Longitude>"); sw.Write(obj.Longitude); sw.WriteLine("</Longitude>");

                        sw.Write("        ");
                        sw.Write("<Latitude>"); sw.Write(obj.Latitude); sw.WriteLine("</Latitude>");

                        sw.Write("        ");
                        sw.Write("<SFX>"); sw.Write(sndObj.SFXName); sw.WriteLine("</SFX>");

                        sw.Write("        ");
                        sw.Write("<Radius>"); sw.Write(sndObj.Radius); sw.WriteLine("</Radius>");



                        sw.Write("    </"); sw.Write(obj.SectionName); sw.WriteLine(@">");
                    }
                }
                sw.WriteLine("</SoundObjects>");
                sw.Close();
                #endregion
            }
        }


        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentImage = (Image)checkedListBox1.SelectedItem;
            pictureBox1.Refresh();
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            if (SelectedObject != null)
            {
                objectList.Remove(SelectedObject);
            }
            pictureBox1.Refresh();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            drawRadius = toolStripButton4.Checked;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (selectedObject != null)
            {
                switch (selectedObject.Type)
                {
                    case ObjectType.City:
                        MapCity city = (MapCity)selectedObject.Tag;

                        city.Name = textBox1.Text;
                        city.FarmCount = (int)numericUpDown2.Value;


                        city.ProblemHunger = (float)numericUpDown1.Value;
                        city.ProblemEducation = (float)numericUpDown3.Value;
                        city.ProblemGender = (float)numericUpDown4.Value;
                        city.ProblemChild = (float)numericUpDown5.Value;
                        city.ProblemMaternal = (float)numericUpDown6.Value;
                        city.ProblemDisease = (float)numericUpDown7.Value;
                        city.ProblemEnvironment = (float)numericUpDown8.Value;

                        if (radioButton5.Checked)
                        {
                            city.Size = UrbanSize.Small;
                        }
                        else if (radioButton6.Checked) 
                        {
                            city.Size = UrbanSize.Medium;
                        }
                        else if (radioButton7.Checked)
                        {
                            city.Size = UrbanSize.Large;
                        }

                        city.StartUp = (int)numericUpDown14.Value;
                        city.LinkableCity = textBox4.Text.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        if (string.IsNullOrEmpty(selectedObject.SectionName))
                        {
                            selectedObject.SectionName = "City" + Guid.NewGuid().ToString("N");
                        }
                        selectedObject.StringDisplay = city.Name;
                        break;
                    case ObjectType.ResOil:
                    case ObjectType.ResWood:
                        MapResource oil = (MapResource)selectedObject.Tag;

                        oil.Amount = (float)numericUpDown9.Value;
                        oil.Radius = (float)numericUpDown10.Value;

                        if (radioButton1.Checked)
                        {
                            oil.Type = NaturalResourceType.Wood;
                        }
                        else if (radioButton2.Checked)
                        {
                            oil.Type = NaturalResourceType.Petro;
                        }
                        if (string.IsNullOrEmpty(selectedObject.SectionName))
                        {
                            selectedObject.SectionName = "Resource" + Guid.NewGuid().ToString("N");
                        }
                        selectedObject.StringDisplay = (selectedObject.Type == ObjectType.ResOil ? "O" : "W") + oil.Amount.ToString();
                        selectedObject.Radius = oil.Radius; 
                        break;
                    case ObjectType.Scene:
                        MapSceneObject so = (MapSceneObject)selectedObject.Tag;

                        so.IsForest = checkBox1.Checked;

                        so.Amount = (float)numericUpDown12.Value;
                        so.Radius = (float)numericUpDown13.Value;
                        so.Model = textBox3.Text;
                        
                        if (string.IsNullOrEmpty(selectedObject.SectionName))
                        {
                            selectedObject.SectionName = "Scene" + Guid.NewGuid().ToString("N");
                        }
                        selectedObject.StringDisplay = so.Model;
                        selectedObject.Radius = so.Radius; 
                        break;
                    case ObjectType.Sound:
                        MapSoundObject sndObj = (MapSoundObject)selectedObject.Tag;

                        sndObj.Radius = (float)numericUpDown11.Value;
                        sndObj.SFXName = comboBox2.Text;

                        if (string.IsNullOrEmpty(selectedObject.SectionName))
                        {
                            selectedObject.SectionName = "Sound" + Guid.NewGuid().ToString("N");
                        }
                        selectedObject.StringDisplay = sndObj.SFXName;
                        selectedObject.Radius = sndObj.Radius; 
                        break;
                }

            }
        }

        private void toolStripButton11_Click(object sender, EventArgs e)
        {
            drawString = toolStripButton11.Checked;
            pictureBox1.Refresh();
        }

        #region filter
        private void toolStripButton10_Click(object sender, EventArgs e)
        {
            if (toolStripButton10.Checked)
            {
                filter |= ObjectType.City;
            }
            else
            {
                filter ^= ObjectType.City;
            }
            pictureBox1.Refresh();
        }

        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            if (toolStripButton9.Checked)
            {
                filter |= ObjectType.ResWood;
            }
            else
            {
                filter ^= ObjectType.ResWood;
            }
            pictureBox1.Refresh();
        }

        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            if (toolStripButton8.Checked)
            {
                filter |= ObjectType.ResOil;
            }
            else
            {
                filter ^= ObjectType.ResOil;
            }
            pictureBox1.Refresh();
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            if (toolStripButton7.Checked)
            {
                filter |= ObjectType.Scene;
            }
            else 
            {
                filter ^= ObjectType.Scene;
            }
            pictureBox1.Refresh();
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            if (toolStripButton6.Checked)
            {
                filter |= ObjectType.Sound;
            }
            else
            {
                filter ^= ObjectType.Sound;
            }
            pictureBox1.Refresh();
        }
        #endregion

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (isDraging && selectedObject != null)
                {
                    selectedObject.X = e.X;
                    selectedObject.Y = e.Y;
                    pictureBox1.Refresh();
                }
            }
            float lng, lat;
            MapObject.GetCoord(e.X, e.Y, out lng, out lat);
            toolStripStatusLabel1.Text = Apoc3D.MathLib.MathEx.Radian2Degree(lat).ToString("F2") + ", " + Apoc3D.MathLib.MathEx.Radian2Degree(lng).ToString("F2");
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < objectList.Count; i++)
            {
                MapObject obj = objectList[i];
                if ((obj.Type & filter) == obj.Type && obj.Intersects(e.X, e.Y))
                {
                    SelectedObject = obj;
                    isDraging = true;
                    pictureBox1.Refresh(); 
                    return;
                }
            }
            SelectedObject = null;
            isDraging = false;
            pictureBox1.Refresh();
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            isDraging = false;
        }

        private void toolStripButton12_Click(object sender, EventArgs e)
        {
            MapObject obj = new MapObject();

            obj.Tag = new MapCity();
            obj.SectionName = "City" + Guid.NewGuid().ToString("N");
            obj.Type = ObjectType.City;
            
            objectList.Add(obj);
            pictureBox1.Refresh();
        }

        private void toolStripButton13_Click(object sender, EventArgs e)
        {
            MapObject obj = new MapObject();

            MapResource mres = new MapResource();
            mres.Type = NaturalResourceType.Wood;
            obj.Tag = new MapResource();
            obj.SectionName = "Resource" + Guid.NewGuid().ToString("N");
            obj.Type = ObjectType.ResWood;

            objectList.Add(obj);
            pictureBox1.Refresh();
        }

        private void toolStripButton14_Click(object sender, EventArgs e)
        {
            MapObject obj = new MapObject();

            MapResource mres = new MapResource();
            mres.Type = NaturalResourceType.Petro;
            obj.Tag = mres;
            obj.SectionName = "Resource" + Guid.NewGuid().ToString("N");
            obj.Type = ObjectType.ResOil;

            objectList.Add(obj);
            pictureBox1.Refresh();
        }

        private void toolStripButton15_Click(object sender, EventArgs e)
        {
            MapObject obj = new MapObject();

            obj.Tag = new MapSceneObject();
            obj.SectionName = "Scene" + Guid.NewGuid().ToString("N");
            obj.Type = ObjectType.Scene;

            objectList.Add(obj);
            pictureBox1.Refresh();
        }

        private void toolStripButton16_Click(object sender, EventArgs e)
        {
            MapObject obj = new MapObject();

            obj.Tag = new MapSoundObject();
            obj.SectionName = "Sound" + Guid.NewGuid().ToString("N");
            obj.Type = ObjectType.Sound;

            objectList.Add(obj);
            pictureBox1.Refresh();
        }

    }
}
