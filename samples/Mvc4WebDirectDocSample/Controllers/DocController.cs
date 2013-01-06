﻿using System;
using System.Diagnostics.Contracts;
using System.Web.Mvc;
using DandyDoc;
using DandyDoc.Overlays.Cref;
using DandyDoc.Overlays.MsdnLinks;
using DandyDoc.ViewModels;
using Mono.Cecil;
using DandyDoc.Overlays.XmlDoc;

namespace Mvc4WebDirectDocSample.Controllers
{
	public class DocController : Controller
	{

		public DocController(
			AssemblyDefinitionCollection assemblyDefinitionCollection,
			CrefOverlay crefOverlay,
			XmlDocOverlay xmlDocOverlay,
			TypeNavigationViewModel typeNavigationViewModel,
			IMsdnLinkOverlay msdnLinkOverlay
		) {
			AssemblyDefinitionCollection = assemblyDefinitionCollection;
			CrefOverlay = crefOverlay;
			XmlDocOverlay = xmlDocOverlay;
			TypeNavigationViewModel = typeNavigationViewModel;
			MsdnLinkOverlay = msdnLinkOverlay;
		}

		public AssemblyDefinitionCollection AssemblyDefinitionCollection { get; private set; }

		public CrefOverlay CrefOverlay { get; private set; }

		public XmlDocOverlay XmlDocOverlay { get; private set; }

		public TypeNavigationViewModel TypeNavigationViewModel { get; private set; }

		public IMsdnLinkOverlay MsdnLinkOverlay { get; private set; }

		public ActionResult Index(string cref) {
			if(String.IsNullOrEmpty(cref))
				return new HttpNotFoundResult();
			var reference = CrefOverlay.GetReference(cref);
			if (null == reference)
				return new HttpNotFoundResult();

			ViewResult viewResult;
			if (reference is TypeDefinition){
				var typeDefinition = (TypeDefinition) reference;
				if (typeDefinition.IsDelegateType()) {
					var delegateViewModel = new DelegateViewModel(typeDefinition, XmlDocOverlay);
					viewResult = View("Delegate", delegateViewModel);
				}
				else {
					var typeViewModel = new TypeViewModel(typeDefinition, XmlDocOverlay);
					viewResult = View(
						typeDefinition.IsEnum ? "Enum" : "Type",
						typeViewModel);
				}
			}
			else if (reference is MethodDefinition) {
				viewResult = View("Method", new MethodViewModel((MethodDefinition)reference, XmlDocOverlay));
			}
			else if (reference is FieldDefinition) {
				viewResult = View("Field", new FieldViewModel((FieldDefinition)reference, XmlDocOverlay));
			}
			else if (reference is PropertyDefinition) {
				viewResult = View("Property", new PropertyViewModel((PropertyDefinition)reference, XmlDocOverlay));
			}
			else if (reference is EventDefinition){
				viewResult = View("Event", new EventViewModel((EventDefinition) reference, XmlDocOverlay));
			}
			else{
				throw new NotSupportedException();
			}
			Contract.Assume(null != viewResult);
			viewResult.ViewBag.TypeNavigationViewModel = TypeNavigationViewModel;
			viewResult.ViewBag.CrefOverlay = CrefOverlay;
			viewResult.ViewBag.MsdnLinkOverlay = MsdnLinkOverlay;
			return viewResult;
		}

	}
}
