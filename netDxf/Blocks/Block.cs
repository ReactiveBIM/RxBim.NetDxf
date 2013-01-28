#region netDxf, Copyright(C) 2009 Daniel Carvajal, Licensed under LGPL.

// netDxf library
// Copyright (C) 2009  
// Daniel Carvajal
// haplokuon@gmail.com
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

#endregion

using System;
using System.Collections.Generic;
using netDxf.Entities;
using netDxf.Tables;

namespace netDxf.Blocks
{
    /// <summary>
    /// Represents a block definition.
    /// </summary>
    public class Block :
        DxfObject 
    {
        #region private fields

        private BlockRecord record;
        private readonly BlockEnd end;
        private readonly string name;
        private BlockTypeFlags typeFlags;
        private Layer layer;
        private Vector3 position;
        private Dictionary<string, AttributeDefinition> attributes;
        private List<IEntityObject> entities;

        #endregion

        #region constants

        internal static Block  ModelSpace
        {
            get{ return new Block("*Model_Space");}
        }

        internal static Block PaperSpace
        {
            get { return new Block("*Paper_Space"); }
        }

        internal static Block PaperSpace0
        {
            get { return new Block("*Paper_Space0"); }
        }
        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Block</c> class.
        /// </summary>
        /// <param name="name">Block name.</param>
        public Block(string name) : base (DxfObjectCode.Block)
        {
            if (string.IsNullOrEmpty(name))
                throw (new ArgumentNullException("name"));
            
            this.name = name;
            this.position = Vector3.Zero;
            this.layer = Layer.Default;
            this.attributes = new Dictionary<string, AttributeDefinition>();
            this.entities = new List<IEntityObject>();
            this.record = new BlockRecord(name);
            this.end = new BlockEnd(this.layer);          
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the block name.
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }

        /// <summary>
        /// Gets or sets the block position in world coordinates.
        /// </summary>
        public Vector3 Position
        {
            get { return this.position; }
            set { this.position = value; }
        }

        /// <summary>
        /// Gets or sets the block <see cref="Layer">layer</see>.
        /// </summary>
        public Layer Layer
        {
            get { return this.layer; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value"); 
                this.layer = value;
                this.end.Layer = value;
            }
        }

        /// <summary>
        /// Gets or sets the block <see cref="AttributeDefinition">attribute definition</see> list.
        /// </summary>
        public Dictionary<string, AttributeDefinition> Attributes
        {
            get { return this.attributes; }
            set
            {
                if (value == null)
                    throw new NullReferenceException("value");
                this.attributes = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IEntityObject">entity</see> list that makes the block.
        /// </summary>
        /// <remarks>
        /// The UCS in effect when a block definition is created becomes the WCS for all
        /// entities in the block definition. The new origin for these entities is shifted to
        /// match the base point defined for the block definition. All entity data is translated to fit this new WCS.
        /// </remarks>
        public List<IEntityObject> Entities
        {
            get { return this.entities; }
            set
            {
                if (value == null)
                    throw new NullReferenceException("value");
                this.entities = value;
            }
        }

        internal BlockRecord Record
        {
            get { return this.record; }
            set { this.record = value; }
        }

        internal BlockEnd End
        {
            get { return this.end; }
        }

        
        #endregion

        #region internal properties

        /// <summary>
        /// Gets or sets the block-type flags (bit-coded values, may be combined).
        /// </summary>
        internal BlockTypeFlags TypeFlags
        {
            get { return typeFlags; }
            set { typeFlags = value; }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Asigns a handle to the object based in a integer counter.
        /// </summary>
        /// <param name="entityNumber">Number to asign.</param>
        /// <returns>Next avaliable entity number.</returns>
        /// <remarks>
        /// Some objects might consume more than one, is, for example, the case of polylines that will asign
        /// automatically a handle to its vertexes. The entity number will be converted to an hexadecimal number.
        /// </remarks>
        internal override int AsignHandle(int entityNumber)
        {
            entityNumber = this.record.AsignHandle(entityNumber);
            entityNumber = this.end.AsignHandle(entityNumber);
            foreach(AttributeDefinition attDef in this.attributes.Values )
            {
                entityNumber = attDef.AsignHandle(entityNumber);
            }
            foreach (IEntityObject entity in this.entities )
            {
                entityNumber = ((DxfObject) entity).AsignHandle(entityNumber);
            }
            return base.AsignHandle(entityNumber);
        }

        /// <summary>
        /// Converts the value of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            return this.name;
        }

        #endregion
    }
}