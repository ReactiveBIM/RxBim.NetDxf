#region netDxf library licensed under the MIT License
// 
//                       netDxf library
// Copyright (c) Daniel Carvajal (haplokuon@gmail.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using netDxf.Collections;
using netDxf.Tables;
using netDxf.IO;

namespace netDxf.Objects
{
    /// <summary>
    /// Represents a layer state.
    /// </summary>
    public class LayerState :
        TableObject
    {
        #region private fields

        private const string LayerStateDictionary = "LAYERSTATEDICTIONARY";
        private const string LayerStateName = "LAYERSTATE";

        private string description;
        private string currentLayer;
        private readonly ObservableDictionary<string, LayerStateProperties> properties;

        #endregion

        #region constructor

        /// <summary>
        /// Initializes a new instance of the <c>LayerState</c> class.
        /// </summary>
        /// <param name="name">Layer state name.</param>
        public LayerState(string name)
            : this(name, new List<Layer>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>LayerState</c> class from a specified list of layers.
        /// </summary>
        /// <param name="name">Layer state name.</param>
        /// <param name="layers">List of layers.</param>
        public LayerState(string name, IEnumerable<Layer> layers)
            : base(name, DxfObjectCode.LayerStates, true)
        {
            description = string.Empty;
            currentLayer = Layer.DefaultName;
            PaperSpace = false;

            properties = new ObservableDictionary<string, LayerStateProperties>();
            properties.BeforeAddItem += this.Properties_BeforeAddItem;

            if (layers == null)
            {
                throw new ArgumentNullException(nameof(layers));
            }

            foreach (var layer in layers)
            {
                var prop = new LayerStateProperties(layer);
                properties.Add(prop.Name, prop);
            }
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the layer state description.
        /// </summary>
        public string Description
        {
            get => description;
            set => description = value ?? string.Empty;
        }

        /// <summary>
        /// Gets or sets the current layer name.
        /// </summary>
        public string CurrentLayer
        {
            get => currentLayer;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (this.Owner != null)
                {
                    if(!this.Owner.Owner.Layers.Contains(value))
                    {
                        throw new ArgumentException("The value cannot be set as the current layer. It does not exist in the document owner of this layer state.", nameof(value));
                    }
                }
                this.currentLayer = value;
            }
        }

        /// <summary>
        /// Gets or sets if the layer state belongs to a paper space layout.
        /// </summary>
        public bool PaperSpace { get; set; }

        /// <summary>
        /// Gets the list of layer state properties.
        /// </summary>
        public ObservableDictionary<string, LayerStateProperties> Properties => properties;

        /// <summary>
        /// Gets the owner of the actual layer state.
        /// </summary>
        public new LayerStateManager Owner
        {
            get => (LayerStateManager) base.Owner;
            internal set => base.Owner = value;
        }

        #endregion

        #region public methods

        /// <summary>
        /// Loads a layer state from an LAS file.
        /// </summary>
        /// <param name="file">LAS file to load.</param>
        /// <returns>A layer state.</returns>
        public static LayerState Load(string file)
        {
            Stream stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            try
            {
                return Read(stream);
            }
            catch
            {
                return null;
            }
            finally
            {
                stream.Close();
            }
        }

        /// <summary>
        /// Saves the current layer state to a LAS file.
        /// </summary>
        /// <param name="file">LAS file to save.</param>
        /// <returns>Returns true if the file has been successfully saved, false otherwise.</returns>
        public bool Save(string file)
        {
            var stream = File.Create(file);
            try
            {
                Write(stream, this);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                stream.Close();
            }
        }

        #endregion

        #region private methods

        private static void Write(Stream stream, LayerState ls)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (ls == null)
            {
                throw new ArgumentNullException(nameof(ls));
            }

            var chunk = new TextCodeValueWriter(new StreamWriter(stream));

            chunk.Write(0, LayerStateDictionary);
            chunk.Write(0, LayerStateName);
            chunk.Write(1, ls.Name);
            chunk.Write(91, 2047); // unknown code functionality <- 32-bit integer value
            chunk.Write(301, ls.Description);
            chunk.Write(290, ls.PaperSpace);
            chunk.Write(302, ls.CurrentLayer);

            foreach (var lp in ls.Properties.Values)
            {
                chunk.Write(8, lp.Name);
                chunk.Write(90, (int) lp.Flags);
                chunk.Write(62, lp.Color.Index);
                chunk.Write(370, (short) lp.Lineweight);
                chunk.Write(6, lp.LinetypeName);
                //chunk.Write(2, properties.PlotStyleName);
                chunk.Write(440, lp.Transparency.Value == 0 ? 0 : Transparency.ToAlphaValue(lp.Transparency));
                if (lp.Color.UseTrueColor)
                {
                    // this code only appears if the layer color has been defined as true color
                    chunk.Write(92, AciColor.ToTrueColorForLayerState(lp.Color));
                }
            }
            chunk.Flush();
        }

        private static LayerState Read(Stream stream)
        {
            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var chunk = new TextCodeValueReader(new StreamReader(stream, Encoding.UTF8, true));

            chunk.Next();
            if (chunk.Code != 0 || chunk.ReadString() != LayerStateDictionary)
            {
                throw new Exception("File not valid.");
            }

            chunk.Next();
            return ReadLayerState(chunk);
        }

        private static LayerState ReadLayerState(TextCodeValueReader chunk)
        {
            var name = string.Empty;
            var description = string.Empty;
            var currentLayer = Layer.DefaultName;
            var paperSpace = false;
            var layerProperties = new List<LayerStateProperties>();

            if (chunk.Code == 0 )
            {
                if (chunk.ReadString() != LayerStateName)
                {
                    throw new Exception("File not valid.");
                }
            }
            else
            {
                throw new Exception("File not valid.");
            }
            chunk.Next();

            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 1:
                        name = chunk.ReadString();
                        chunk.Next();
                        break;
                    case 91: // unknown code
                        chunk.Next();
                        break;
                    case 301:
                        description = chunk.ReadString();
                        chunk.Next();
                        break;
                    case 290:
                        paperSpace = chunk.ReadBool();
                        chunk.Next();
                        break;
                    case 302: // active layer
                        currentLayer = chunk.ReadString();
                        chunk.Next();
                        break;
                    case 8: // begin reading layer properties
                        layerProperties.Add(ReadLayerProperties(chunk));
                        break;
                    default:
                        chunk.Next();
                        break;
                }
            }

            var states = new LayerState(name)
            {
                Description = description,
                CurrentLayer = currentLayer,
                PaperSpace = paperSpace
            };

            foreach (var lp in layerProperties.Where(lp => !states.Properties.ContainsKey(lp.Name)))
            {
                states.Properties.Add(lp.Name, lp);
            }

            return states;
        }

        private static LayerStateProperties ReadLayerProperties(TextCodeValueReader chunk)
        {
            var flags = LayerPropertiesFlags.Plot;
            var lineType = Linetype.DefaultName;
            //string plotStyle = string.Empty;
            var color = AciColor.Default;
            var lineWeight = Lineweight.Default;
            var transparency = new Transparency(0);

            var name = chunk.ReadString();
            chunk.Next();

            while (chunk.Code != 8 && chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 90:
                        flags = (LayerPropertiesFlags) chunk.ReadInt();
                        chunk.Next();
                        break;
                    case 62:
                        color = AciColor.FromCadIndex(chunk.ReadShort());
                        chunk.Next();
                        break;
                    case 370:
                        lineWeight = (Lineweight) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 6:
                        lineType = chunk.ReadString();
                        chunk.Next();
                        break;
                    case 2:
                        //plotStyle = chunk.ReadString();
                        chunk.Next();
                        break;
                    case 440:
                        var alpha = chunk.ReadInt();
                        transparency = alpha == 0 ? new Transparency(0) : Transparency.FromAlphaValue(alpha);
                        chunk.Next();
                        break;
                    case 92:
                        color = AciColor.FromTrueColor(chunk.ReadInt());
                        chunk.Next();
                        break;
                    default:
                        chunk.Next();
                        break;
                }
            }

            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            var properties = new LayerStateProperties(name)
            {
                Flags = flags,
                Color = color,
                Lineweight = lineWeight,
                LinetypeName = lineType,
                //PlotStyleName = plotStyle,
                Transparency = transparency
            };

            return properties;
        }

        #endregion

        #region overrides

        /// <summary>
        /// Checks if this instance has been referenced by other DxfObjects. 
        /// </summary>
        /// <returns>
        /// Returns true if this instance has been referenced by other DxfObjects, false otherwise.
        /// It will always return false if this instance does not belong to a document.
        /// </returns>
        /// <remarks>
        /// This method returns the same value as the HasReferences method that can be found in the TableObjects class.
        /// </remarks>
        public override bool HasReferences()
        {
            return Owner != null && Owner.HasReferences(Name);
        }

        /// <summary>
        /// Gets the list of DxfObjects referenced by this instance.
        /// </summary>
        /// <returns>
        /// A list of DxfObjectReference that contains the DxfObject referenced by this instance and the number of times it does.
        /// It will return null if this instance does not belong to a document.
        /// </returns>
        /// <remarks>
        /// This method returns the same list as the GetReferences method that can be found in the TableObjects class.
        /// </remarks>
        public override List<DxfObjectReference> GetReferences()
        {
            return Owner?.GetReferences(Name);
        } // TODO: Check this

        /// <summary>
        /// Creates a new LayerState that is a copy of the current instance.
        /// </summary>
        /// <param name="newName">LayerState name of the copy.</param>
        /// <returns>A new LayerState that is a copy of this instance.</returns>
        public override TableObject Clone(string newName)
        {
            var ls = new LayerState(newName)
            {
                Description = this.description,
                CurrentLayer = this.currentLayer
            };

            foreach (var item in this.properties.Values)
            {
                var lp = (LayerStateProperties) item.Clone();
                ls.Properties.Add(lp.Name, lp);
            }

            return ls;
        }

        /// <summary>
        /// Creates a new LayerState that is a copy of the current instance.
        /// </summary>
        /// <returns>A new LayerState that is a copy of this instance.</returns>
        public override object Clone()
        {
            return Clone(Name);
        }

        #endregion

        #region Properties events

        private void Properties_BeforeAddItem(ObservableDictionary<string, LayerStateProperties> sender, ObservableDictionaryEventArgs<string, LayerStateProperties> e)
        {
            if (e.Item.Value == null)
            {
                e.Cancel = true;
            }
            else if (Owner != null)
            {
                var doc = Owner.Owner;
                if (!doc.Layers.Contains(e.Item.Key))
                {
                    e.Cancel = true;
                }

                if (!doc.Linetypes.Contains(e.Item.Value.LinetypeName))
                {
                    e.Cancel = true;
                }
            }
            else
            {
                e.Cancel = false;
            }
        }


        #endregion
    }
}