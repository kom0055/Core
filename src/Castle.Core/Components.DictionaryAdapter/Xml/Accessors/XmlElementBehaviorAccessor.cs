﻿// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.f
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.Components.DictionaryAdapter.Xml
{
	using System;
	using System.Collections.Generic;
	using System.Xml.Serialization;

	public class XmlElementBehaviorAccessor : XmlNodeAccessor,
		IConfigurable<XmlElementAttribute>,
		IXmlTypeFrom <XmlElementAttribute>
	{
		private ItemAccessor itemAccessor;
		private List<XmlElementAttribute> attributes;

		internal static readonly XmlAccessorFactory<XmlElementBehaviorAccessor>
			Factory = (property, context) => new XmlElementBehaviorAccessor(property, context);

		public XmlElementBehaviorAccessor(PropertyDescriptor property, IXmlAccessorContext context)
			: base(property, context) { }

		public void Configure(XmlElementAttribute attribute)
		{
			if (attribute.Type == null)
			{
				ConfigureLocalName   (attribute.ElementName);
				ConfigureNamespaceUri(attribute.Namespace  );
				ConfigureNillable    (attribute.IsNullable );
			}
			else
			{
				if (attributes == null)
					attributes = new List<XmlElementAttribute>();
				attributes.Add(attribute);
			}
		}

		public override void Prepare()
		{
			if (attributes != null)
			{
				ConfigureKnownTypesFromAttributes(attributes, this);
				attributes = null;				
			}
			base.Prepare();
		}

		protected override void SetPropertyToNull(IXmlCursor cursor)
		{
			if (IsCollection)
				base.RemoveCollectionItems(cursor);
			else
				base.SetPropertyToNull(cursor);
		}

		public override IXmlCollectionAccessor GetCollectionAccessor(Type itemType)
		{
			return itemAccessor ?? (itemAccessor = new ItemAccessor(this));
		}

		public override IXmlCursor SelectPropertyNode(IXmlNode node, bool mutable)
		{
			return node.SelectChildren(KnownTypes, CursorFlags.Elements.MutableIf(mutable));
		}

		public override IXmlCursor SelectCollectionNode(IXmlNode node, bool mutable)
		{
			return node.SelectSelf(ClrType);
		}

		public string GetLocalName(XmlElementAttribute attribute)
		{
			return attribute.ElementName;
		}

		public string GetNamespaceUri(XmlElementAttribute attribute)
		{
			return attribute.Namespace;
		}

		public string GetXsiType(XmlElementAttribute attribute)
		{
			return attribute.Type.GetLocalName();
		}

		public Type GetClrType(XmlElementAttribute attribute)
		{
			return attribute.Type;
		}

		private class ItemAccessor : XmlNodeAccessor
		{
			public ItemAccessor(XmlNodeAccessor parent)
				: base(parent.ClrType.GetCollectionItemType(), parent.Context)
			{
				ConfigureLocalName   (parent.LocalName   );
				ConfigureNamespaceUri(parent.NamespaceUri);
				ConfigureNillable    (parent.IsNillable  );
				ConfigureKnownTypesFromParent(parent);
			}

			public override void Prepare()
			{
				// Don't prepare; parent already did it
			}

			public override IXmlCursor SelectCollectionItems(IXmlNode node, bool mutable)
			{
				return node.SelectChildren(KnownTypes, CursorFlags.Elements.MutableIf(mutable) | CursorFlags.Multiple);
			}
		}
	}
}