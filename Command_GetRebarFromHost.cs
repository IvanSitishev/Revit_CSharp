using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using System.Diagnostics;
using System.IO;
//using RoomFinishing;
//using FireSharp.Config;
//using FireSharp.Interfaces;
//using FireSharp.Response;
using System.Threading.Tasks;
using System.Reflection;
using Autodesk.Revit.DB.Structure;

namespace RebarUtils
{
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class Command_GetRebarFromHost : IExternalCommand
    {


        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uidoc = uiApp.ActiveUIDocument;
            Application app = uiApp.Application;
            Document doc = uiApp.ActiveUIDocument.Document;

            IList<Reference> sel = uidoc.Selection.PickObjects(ObjectType.Element, "Выберете элементы, содержащие арматуру");
            List < ElementId > list_rebars = new List<ElementId>();

            foreach(Reference refer in sel)
            {
                ElementId elId = refer.ElementId;
                FilteredElementCollector collectorRebar = new FilteredElementCollector(doc).OfClass(typeof(Rebar)).WhereElementIsNotElementType();
                FilteredElementCollector collectorRebarInSystem = new FilteredElementCollector(doc).OfClass(typeof(RebarInSystem)).WhereElementIsNotElementType();
                FilteredElementCollector collectorPathReinforc = new FilteredElementCollector(doc).OfClass(typeof(PathReinforcement)).WhereElementIsNotElementType();
                FilteredElementCollector collectorAreaReinforce = new FilteredElementCollector(doc).OfClass(typeof(AreaReinforcement)).WhereElementIsNotElementType();
                var lst = from Rebar r in collectorRebar.ToElements() where r.GetHostId() == elId select r.Id;
                list_rebars.AddRange(lst);
                lst = from RebarInSystem rs in collectorRebarInSystem.ToElements() where rs.GetHostId() == elId select rs.Id;
                list_rebars.AddRange(lst);
                lst = from PathReinforcement rs in collectorPathReinforc.ToElements() where rs.GetHostId() == elId select rs.Id;
                list_rebars.AddRange(lst);
                lst = from AreaReinforcement rs in collectorAreaReinforce.ToElements() where rs.GetHostId() == elId select rs.Id;
                list_rebars.AddRange(lst);

            }

            uidoc.Selection.SetElementIds(list_rebars);

            return Result.Succeeded;
        }
    }

    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class Command_GetHostFromRebar : IExternalCommand
    {
        internal sealed class SelectionFilterRebar : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                if (elem is null) return false;

                //if (!(elem is FamilyInstance)) return false;

                if (elem is FamilyInstance) return false;

                BuiltInCategory builtInCategory = (BuiltInCategory)GetCategoryIdAsInteger(elem);

                if (builtInCategory == BuiltInCategory.OST_Rebar)
                    
                    return true;

                return false;
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                return false;
            }

            private int GetCategoryIdAsInteger(Element element)
            {
                return element?.Category?.Id?.IntegerValue ?? -1;
            }
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uidoc = uiApp.ActiveUIDocument;
            Application app = uiApp.Application;
            Document doc = uiApp.ActiveUIDocument.Document;

            IList<Reference> sel = uidoc.Selection.PickObjects(ObjectType.Element, new SelectionFilterRebar(), "Выберете арматурный стержень");
            List<ElementId> list_host = new List<ElementId>();

            foreach (Reference refer in sel)
            {
                ElementId elId = refer.ElementId;
                Element el = doc.GetElement(elId);
                Element host = null;
                try
                {
                    Rebar r = (Rebar)el;
                    host = doc.GetElement(r.GetHostId());
                }
                catch
                {
                    RebarInSystem rs = (RebarInSystem)el;
                    host = doc.GetElement(rs.GetHostId()); 
                }

                //FilteredElementCollector collectorRebar = new FilteredElementCollector(doc).OfClass(typeof(Rebar)).WhereElementIsNotElementType();
                //var lst = from Rebar r in collectorRebar.ToElements() where r.GetHostId() == elId select r.Id;
                if (!list_host.Contains(host.Id))
                {
                    list_host.Add(host.Id);
                }

            }

            uidoc.Selection.SetElementIds(list_host);

            return Result.Succeeded;
        }
    }


    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Command_GetRebarSystem : IExternalCommand
    {
        internal sealed class SelectionFilterRebar : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                if (elem is null) return false;

                //if (!(elem is FamilyInstance)) return false;

                if (elem is FamilyInstance) return false;

                BuiltInCategory builtInCategory = (BuiltInCategory)GetCategoryIdAsInteger(elem);

                if (builtInCategory == BuiltInCategory.OST_Rebar)

                    return true;

                return false;
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                return false;
            }

            private int GetCategoryIdAsInteger(Element element)
            {
                return element?.Category?.Id?.IntegerValue ?? -1;
            }
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uidoc = uiApp.ActiveUIDocument;
            Application app = uiApp.Application;
            Document doc = uiApp.ActiveUIDocument.Document;

            IList<Reference> sel = uidoc.Selection.PickObjects(ObjectType.Element, new SelectionFilterRebar(), "Выберете арматурный стержень");
            List<ElementId> list_host = new List<ElementId>();

            foreach (Reference refer in sel)
            {
                ElementId elId = refer.ElementId;
                Element el = doc.GetElement(elId);
                ElementId hostId = null;

                if (el is Rebar) continue;
                else if (el is RebarInSystem)
                {
                    RebarInSystem rs = (RebarInSystem)el;
                    hostId = rs.SystemId;
                }
                //try
                //{
                //    Rebar r = (Rebar)el;
                //    host = doc.GetElement(r.GetHostId());
                //}
                //catch
                //{
                //    RebarInSystem rs = (RebarInSystem)el;
                //    host = doc.GetElement(rs.GetHostId());
                //}

                //FilteredElementCollector collectorRebar = new FilteredElementCollector(doc).OfClass(typeof(Rebar)).WhereElementIsNotElementType();
                //var lst = from Rebar r in collectorRebar.ToElements() where r.GetHostId() == elId select r.Id;
                if (!list_host.Contains(hostId))
                {
                    list_host.Add(hostId);
                }

            }

            uidoc.Selection.SetElementIds(list_host);

            return Result.Succeeded;
        }

    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Command_SetExpense : IExternalCommand
    {
        public static List<Rebar> GetRebarFromHost(Element host)
        {
            List<ElementId> list_rebars = new List<ElementId>();
            ElementId elId = host.Id;
            FilteredElementCollector collectorRebar = new FilteredElementCollector(host.Document).OfClass(typeof(Rebar)).WhereElementIsNotElementType();
            var lst = from Rebar r in collectorRebar.ToElements() where r.GetHostId() == elId select r;
            //list_rebars.AddRange(lst);
            return lst.ToList();
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {


            return Result.Succeeded;
        }
    }
}


