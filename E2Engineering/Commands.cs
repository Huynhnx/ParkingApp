using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using ParkingApp.ViewModels;
using ParkingApp.Views;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace ParkingApp
{
    public class PointInfor
    {
        Point3d pt;
        int type;
    }
    public class Commands
    {
        [CommandMethod("ParkArcIns")]
        public static void ParkingAngleInsert()
        {
            Document acDoc = AcadApp.DocumentManager.MdiActiveDocument;
            ParkingAngle view = new ParkingAngle();
            ParkingAngleViewModel vm = new ParkingAngleViewModel();
            ObjectIdCollection ids = new ObjectIdCollection();
            //int max = 0;
            //ArxHelper.GetAllEntities(ids);
            //using (Transaction tr = acDoc.Database.TransactionManager.StartTransaction())
            //{
            //    foreach (ObjectId id in ids)
            //    {
            //        AttributeDefinition ar = tr.GetObject(id, OpenMode.ForRead) as AttributeDefinition;
            //        if (ar != null)
            //        {

            //            try
            //            {
            //                int num = Int32.Parse(ar.TextString);
            //                if (max < num)
            //                {
            //                    max = num;
            //                }
            //            }
            //            catch
            //            {

            //            }

            //        }
            //    }
            //}
            //if (max == 0)
            //{
            //    max = 1001;
            //}
            try
            {
                vm.StartNumber = Int32.Parse(File.ReadAllText(@"C:\\ParkingApp\\NumFile.txt"));
            }
            catch
            {
                vm.StartNumber = 1001;
            }
            vm.CreateArcCmd = new MVVMCore.RelayCommand(CreateArc);
            vm.OkCmd = new MVVMCore.RelayCommand(OkCmdInvoke);
            vm.CancelCmd = new MVVMCore.RelayCommand(CancekCmdInvoke);
            view.DataContext = vm;

            AcadApp.ShowModalWindow(view);
        }
        private static void CreateArc(object obj)
        {
            ParkingAngle view = obj as ParkingAngle;
            ParkingAngleViewModel vm = view.DataContext as ParkingAngleViewModel;
            Task<ObjectId> id = CreateArcAndSelect(obj);
            id.Wait();
            vm.objId = id.Result;
            view.Show();
        }
       async private static Task<ObjectId> CreateArcAndSelect(object obj)
        {
            ParkingAngle view = obj as ParkingAngle;
            ParkingAngleViewModel vm = view.DataContext as ParkingAngleViewModel;
            if (view != null)
            {
                view.Hide();
                Document doc = AcadApp.DocumentManager.MdiActiveDocument;
                Database db = doc.Database;
                Editor ed = doc.Editor;
                using (doc.LockDocument())
                {
                    await ed.CommandAsync(
                        "_.ARC");
                    return ed.SelectLast().Value.GetObjectIds()[0];
                    //view.Show();
                }
            }
            return ObjectId.Null;
        }

        private static void CancekCmdInvoke(object obj)
        {
            ParkingAngle view = obj as ParkingAngle;
            view.Close();
        }

        private static void OkCmdInvoke(object obj)
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            ParkingAngle view = obj as ParkingAngle;
            ParkingAngleViewModel vm = view.DataContext as ParkingAngleViewModel;
            DBObjectCollection ids = new DBObjectCollection();
            if (vm != null)
            {
                if (vm.NumberOfSpot == 0)
                {
                    MessageBox.Show("Number of Spot should be greater than 0");
                    return;
                }
                using (DocumentLock doclock = doc.LockDocument())
                {
                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        ArxHelper.CreateLayer("Wheel Stops");
                        ArxHelper.CreateLayer("Parking Lines");
                        ArxHelper.CreateLayer("Parking Spaces");
                        double r = 0;
                        Point3dCollection pts = new Point3dCollection();
                        BlockTable acBlkTbl;
                        acBlkTbl = tr.GetObject(db.BlockTableId,
                                                     OpenMode.ForRead) as BlockTable;
                        // Open the Block table record Model space for write
                        BlockTableRecord acBlkTblRec;
                        acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                        OpenMode.ForWrite) as BlockTableRecord;
                        Arc selectArc = tr.GetObject(vm.objId, OpenMode.ForWrite) as Arc;
                        selectArc.Layer = "Parking Lines";
                        DBObjectCollection offset1 = selectArc.GetOffsetCurves(-vm.ParkingPointOffset);
                        DBObjectCollection offset2 = selectArc.GetOffsetCurves(-vm.WheelStopOffset);
                        DBObjectCollection offset3 = selectArc.GetOffsetCurves(-vm.ParkingDepth);
                        IDictionary<Point3d, string> markpoints1 = new Dictionary<Point3d, string>();
                        IDictionary<Point3d, string> markpoints2 = new Dictionary<Point3d, string>();
                        foreach (Arc acEnt in offset1)
                        {
                            for (int i = 0; i <= vm.NumberOfSpot; i++)
                            {
                                Point3d pt1 = acEnt.GetPointAtDist(i * (acEnt.Length / vm.NumberOfSpot));
                                markpoints2.Add(pt1, "ParkingPoint");
                            }
                        }
                       
                        foreach (Arc acEnt in offset3)
                        {
                            
                            for (int i = 0; i <= vm.NumberOfSpot; i++)
                            {
                                Point3d pt1 = acEnt.GetPointAtDist(i * (acEnt.Length / vm.NumberOfSpot));
                                Point3d pt2 = selectArc.GetPointAtDist(i * (selectArc.Length / vm.NumberOfSpot));
                                Line line1 = new Line(pt1, pt2);
                                line1.Layer = "Parking Lines";
                                markpoints1.Add(pt1, "ParkingPoint");
                                ids.Add(line1);
                                
                            }
                           
                        }

                        foreach (Arc acEnt in offset2)
                        {
                            r = acEnt.Radius;

                            double angle = 2 * Math.Asin(vm.WheelStopLength / (2 * r));
                            double arclength = angle * r;
                            for (int i = 0; i <= vm.NumberOfSpot; i++)
                            {
                                Point3d pt1 = acEnt.GetPointAtDist(i * (acEnt.Length / vm.NumberOfSpot));
                                pts.Add(pt1);
                                //markpoints2.Add(pt1, "ParkingPoint");
                            }
                            DBObjectCollection listArc = acEnt.GetSplitCurves(pts);
                            for (int i = 0; i < listArc.Count; i++)
                            {
                                Arc arc = listArc[i] as Arc;
                                if (vm.ParkingLinePointStart == false && i == 0)
                                {
                                    continue;
                                }
                                if (vm.ParkingLinePointEnd == false && i == listArc.Count - 1)
                                {
                                    continue;
                                }
                                try
                                {
                                    Point3d mid = arc.GetPointAtDist(arc.Length / 2);
                                    Point3d ptstart = arc.GetPointAtDist(Math.Abs(arc.Length - arclength) / 2);
                                    Point3d ptEnd = arc.GetPointAtDist(Math.Abs((arc.Length - arclength) / 2 + arclength));
                                    Line l1 = new Line(ptstart, ptEnd);
                                    l1.Layer = "Wheel Stops";
                                    markpoints2.Add(ptstart, "WheelStopPoint");
                                    markpoints2.Add(ptEnd, "WheelStopPoint");
                                    Vector3d dir = -(ptstart - ptEnd).GetPerpendicularVector().GetNormal();
                                    Point3d pt3 = ptstart + dir * vm.WheelStopDepth;
                                    Line l2 = new Line(ptstart, pt3);
                                    l2.Layer = "Wheel Stops";
                                    Point3d pt4 = ptEnd + dir * vm.WheelStopDepth;
                                    Line l3 = new Line(ptEnd, pt4);
                                    l3.Layer = "Wheel Stops";
                                    Line l34 = new Line(pt3, pt4);
                                    l34.Layer = "Wheel Stops";
                                    ids.Add(l2);
                                    ids.Add(l3);
                                    ids.Add(l34);
                                    ids.Add(l1);
                                }
                                catch
                                {
                                    MessageBox.Show("Wheel Stop Length is too big");
                                    view.Close();
                                    return;
                                }
                                  
                            }
                            int num = vm.StartNumber;
                            //markpoints2 = markpoints2.OrderBy(x => Math.Atan2(x.Key.X, x.Key.Y)).ToDictionary(x => x.Key, x => x.Value);
                            //markpoints1 = markpoints1.OrderBy(x => Math.Atan2(x.Key.X, x.Key.Y)).ToDictionary(x => x.Key, x => x.Value);
                            markpoints2 = SortPointsCCW(markpoints2, selectArc.Center);
                            markpoints1 = SortPointsCCW(markpoints1, selectArc.Center);
                            int j = 0;
                           
                            foreach (var pt in markpoints2)
                            {
                                if (vm.WheelStopRequired && vm.WheelStopPointStart == false && j == 0)
                                {
                                    j++;
                                    continue;
                                }
                                if (vm.WheelStopRequired && vm.WheelStopPointEnd == false && j == markpoints2.Count -1)
                                {
                                    j++;
                                    continue;
                                }
                                ObjectId id = InsertCAEXtabBlockReference(pt.Key, num,pt.Value);
                                BlockReference bl = tr.GetObject(id, OpenMode.ForRead) as BlockReference;
                                if (pt.Value == "WheelStopPoint")
                                {
                                    bl.Layer = "Wheel Stops";
                                }
                                else

                                {
                                    bl.Layer = "Parking Spaces";
                                }    
                                ids.Add(bl);
                                j++;
                                num++;
                            }
                            int endNum = markpoints2.Count + markpoints1.Count + vm.StartNumber-1;
                            j = markpoints1.Count - 1;
                            foreach (var pt in markpoints1)
                            {
                                if (vm.WheelStopRequired && vm.WheelStopPointStart == false && j == 0)
                                {
                                    j--;
                                    continue;
                                }
                                if (vm.WheelStopRequired && vm.WheelStopPointEnd == false && j == markpoints1.Count - 1)
                                {
                                    j--;
                                    continue;
                                }
                                ObjectId id = InsertCAEXtabBlockReference(pt.Key, endNum, pt.Value);
                                BlockReference bl = tr.GetObject(id, OpenMode.ForRead) as BlockReference;
                                if (pt.Value == "WheelStopPoint")
                                {
                                    bl.Layer = "Wheel Stops";
                                }
                                else

                                {
                                    bl.Layer = "Parking Spaces";
                                }
                                ids.Add(bl);
                                j--;
                                endNum--;
                            }
                            File.WriteAllText(@"C:\\ParkingApp\\NumFile.txt", string.Empty);
                            File.WriteAllText(@"C:\\ParkingApp\\NumFile.txt", endNum.ToString());
                            // Add each offset object
                            //ids.Add(acEnt);
                            //ids.Add(acBlkTblRec.AppendEntity(acEnt));
                            ids.Add(selectArc);
                            //tr.AddNewlyCreatedDBObject(acEnt, true);
                            CreateBlock(ids);
                        }
                        bool bFixErrors = true;
                        //to show the message in commandline or not
                        bool becho = false;

                        //call audit API
                        doc.Database.Audit(bFixErrors, becho);
                        tr.Commit();
                    }
                }
                view.Close();
            }
        }
        static IDictionary<Point3d, string> SortPointsCCW(IDictionary<Point3d, string> points,Point3d centerpoint)
        {
            
            return points.OrderBy(pt => -centerpoint.GetVectorTo(pt.Key).AngleOnPlane(new Plane())).ToDictionary(x => x.Key, x => x.Value);
        }
        
        public static ObjectId CreateBlock(List<Entity> entities)
        {
            ObjectId blkId = ObjectId.Null;
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            using (doc.LockDocument())
            {
                Transaction tr = db.TransactionManager.StartTransaction();
                using (tr)
                {
                    // Get the block table from the drawing                
                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    string blkName = "Parking";


                    string tempName = blkName;
                    int n = 0;

                    while (bt.Has(tempName))
                    {
                        n++;
                        tempName = blkName + n;
                    }

                    blkName = tempName;
                    if (bt.Has(blkName))
                    {
                        blkId = bt[blkName];
                    }
                    else
                    {
                        // Create our new block table record...                    
                        BlockTableRecord btr = new BlockTableRecord();
                        // ... and set its properties                    
                        btr.Name = blkName;
                        btr.Origin = Point3d.Origin;
                        // Add the new block to the block table                    
                        bt.UpgradeOpen();
                        blkId = bt.Add(btr);

                        foreach (Entity ent in entities)
                        {
                            try
                            {
                                btr.AppendEntity(ent);
                            }
                            catch
                            {
                                Entity en = ent.Clone() as Entity;
                                btr.AppendEntity(en);
                                ent.Erase();
                            }
                        }
                        tr.AddNewlyCreatedDBObject(btr, true);

                        tr.Commit();
                    }
                }
            }
            return blkId;
        }
        public static ObjectId CreateCAEXtabBlock(string blkName)
        {
            ObjectId blkId = ObjectId.Null;
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            using (doc.LockDocument())
            {
                Transaction tr = db.TransactionManager.StartTransaction();
                using (tr)
                {
                    // Get the block table from the drawing                
                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    if (bt.Has(blkName))
                    {
                        blkId = bt[blkName];
                    }
                    else
                    {
                        // Create our new block table record...                    
                        BlockTableRecord btr = new BlockTableRecord();
                        // ... and set its properties                    
                        btr.Name = blkName;
                        // Add the new block to the block table                    
                        bt.UpgradeOpen();
                        blkId = bt.Add(btr);
                        tr.AddNewlyCreatedDBObject(btr, true);
                        DBPoint Point = new DBPoint(Point3d.Origin);
                        btr.AppendEntity(Point);
                        tr.AddNewlyCreatedDBObject(Point, true);
                        tr.Commit();
                    }
                }
            }
            return blkId;
        }
        public static ObjectId InsertCAEXtabBlockReference(Point3d pt, int attribute,string blkname)
        {
            ObjectId blkCAEXtabId = CreateCAEXtabBlock(blkname);
            ObjectId blkRefId = ObjectId.Null;
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            using (doc.LockDocument())
            {
                Database db = doc.Database;
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    // Get the block table from the drawing
                    BlockTable btReds = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    if (btReds != null)
                    {
                        // Add a block reference to the model space
                        BlockTableRecord msRed = (BlockTableRecord)tr.GetObject(btReds[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                        if (msRed != null)
                        {
                            BlockReference cemBlkRef = new BlockReference(pt, blkCAEXtabId);
                            blkRefId = msRed.AppendEntity(cemBlkRef);
                            tr.AddNewlyCreatedDBObject(cemBlkRef, true);
                            // AttributeDefinitions
                            AttributeDefinition attDef = new AttributeDefinition(Point3d.Origin, attribute.ToString(), "Num", "", db.Textstyle);
                            msRed.AppendEntity(attDef);
                            //myT.AddNewlyCreatedDBObject(attDef, true);
                            using (AttributeReference attRef = new AttributeReference())
                            {
                                attRef.SetAttributeFromBlock(attDef, cemBlkRef.BlockTransform);
                                //Add the AttributeReference to the BlockReference
                                cemBlkRef.AttributeCollection.AppendAttribute(attRef);
                                tr.AddNewlyCreatedDBObject(attRef, true);
                            }
                        }
                    }
                    // Commit the transaction
                    tr.Commit();
                }
            }
            return blkRefId;
        }
        public static void CreateBlock(DBObjectCollection ents)

        {

            Document doc = AcadApp.DocumentManager.MdiActiveDocument;

            Database db = doc.Database;

            Editor ed = doc.Editor;

            Transaction tr = db.TransactionManager.StartTransaction();
            using (tr)
            {
                // Get the block table from the drawing
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                // Check the block name, to see whether it's
                // already in use

                BlockTableRecord btr = new BlockTableRecord();

                // ... and set its properties

                string blkName = "ParkingSpot_" +DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString()+ DateTime.Now.Day.ToString() +
                    DateTime.Now.Hour.ToString()+ DateTime.Now.Minute.ToString()+ DateTime.Now.Second.ToString();


                string tempName = blkName;
                int n = 0;

                while (bt.Has(tempName))
                {
                    n++;
                    tempName = blkName + n;
                }
                
                blkName = tempName;

                btr.Name = blkName;
                // Add the new block to the block table

                bt.UpgradeOpen();

                ObjectId btrId = bt.Add(btr);
                tr.AddNewlyCreatedDBObject(btr, true);
                // Add some lines to the block to form a square
                // (the entities belong directly to the block)

                foreach (Entity ent in ents)

                {

                    //btr.AppendEntity(ent);

                    //tr.AddNewlyCreatedDBObject(ent, true);
                    try
                    {
                        btr.AppendEntity(ent);
                        tr.AddNewlyCreatedDBObject(ent, true);
                    }
                    catch
                    {
                        Entity en = ent.Clone() as Entity;
                        btr.AppendEntity(en);
                        tr.AddNewlyCreatedDBObject(en, true);
                        ent.Erase();
                    }

                }
                // Add a block reference to the model space

                BlockTableRecord ms =

                  (BlockTableRecord)tr.GetObject(

                    bt[BlockTableRecord.ModelSpace],

                    OpenMode.ForWrite

                  );

                BlockReference br =

                  new BlockReference(Point3d.Origin, btrId);

                ms.AppendEntity(br);

                tr.AddNewlyCreatedDBObject(br, true);

                tr.Commit();
            }

        }



        [CommandMethod("ParkExp")]
        public static void ExportToCSV()
        {
            ExportView view = new ExportView();
            ExportViewModel vm = new ExportViewModel();
            vm.ExportCmd = new MVVMCore.RelayCommand(CSVHelper.ExportCmdInvoke);
            vm.SelectAdditionPointCmd = new MVVMCore.RelayCommand(SelectAdditionPointCmdInvoke);
            view.DataContext = vm;
            AcadApp.ShowModalWindow(view);
        }

        private static void CancelCmdInvoke(object obj)
        {
            throw new NotImplementedException();
        }

        //private static void SelectParkingSpotCmdInvoke(object obj)
        //{
        //    ExportView view = obj as ExportView;
        //    view.Hide();
        //    Document doc = AcadApp.DocumentManager.MdiActiveDocument;
        //    Database db = doc.Database;

        //    Editor ed = doc.Editor;
        //    ExportViewModel vm = view.DataContext as ExportViewModel;
        //    view.Hide();
        //    var opt = new PromptEntityOptions(
        //            "\nSelect Parking Spot:");
        //    opt.SetRejectMessage("\nInvalid: not an BlockReference.");
        //    opt.AddAllowedClass(typeof(Autodesk.AutoCAD.DatabaseServices.BlockReference), true);
        //    var res = ed.GetEntity(opt);
        //    if (res.Status == PromptStatus.OK)
        //    {
        //        vm.blkId = res.ObjectId;
        //    }
        //    view.Show();
        //}

        private static void SelectAdditionPointCmdInvoke(object obj)
        {
            throw new NotImplementedException();
        }


    }

}
