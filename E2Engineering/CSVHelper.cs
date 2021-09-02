using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using ParkingApp.ViewModels;
using ParkingApp.Views;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using DataTable = System.Data.DataTable;

namespace ParkingApp
{
    public class CSVHelper
    {
        public static void ExportCmdInvoke(object obj)
        {
            ObjectIdCollection ids = new ObjectIdCollection();
            ArxHelper.GetAllEntities(ids);
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    Document doc = AcadApp.DocumentManager.MdiActiveDocument;
                    Database db = doc.Database;

                    Editor ed = doc.Editor;
                    ExportView view = obj as ExportView;
                    ExportViewModel vm = view.DataContext as ExportViewModel;
                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        if (vm.Combine)
                        {
                            System.Data.DataTable table = createDataTable();
                            foreach (ObjectId blkid in ids)
                            {

                                if (blkid.IsValid == false)
                                {
                                    continue;
                                }
                                BlockReference blk = tr.GetObject(blkid, OpenMode.ForRead) as BlockReference;
                                if (blk == null)
                                {
                                    continue;
                                }
                                BlockTableRecord blockDef = tr.GetObject(blk.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
                                foreach (var id in blockDef)
                                {
                                    BlockReference point = tr.GetObject(id, OpenMode.ForRead) as BlockReference;
                                    if (point != null)
                                    {

                                        ObjectId att = point.AttributeCollection[0];
                                        //AttributeDefinition at = tr.GetObject(att, OpenMode.ForRead) as AttributeDefinition;
                                        AttributeReference ar = tr.GetObject(att, OpenMode.ForRead) as AttributeReference;
                                        int num = Int32.Parse(ar.TextString);
                                        double X = point.Position.X;
                                        double Y = point.Position.Y;
                                        double Z = point.Position.Z;
                                        string name = point.Name;
                                        table.Rows.Add(num, X, Y, Z, name);
                                    }
                                }
                            }
                            ToCSV(table, fbd.SelectedPath+"\\WheelStopPointAndParkingPoint.csv");
                        }
                        else if (vm.WheelStopPoint && vm.ParkingPoint == false)
                        {
                            System.Data.DataTable table = createDataTable();
                            foreach (ObjectId blkid in ids)
                            {
                                if (blkid.IsValid == false)
                                {
                                    continue;
                                }
                                BlockReference blk = tr.GetObject(blkid, OpenMode.ForRead) as BlockReference;
                                if (blk == null)
                                {
                                    continue;
                                }
                                
                                BlockTableRecord blockDef = tr.GetObject(blk.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
                                foreach (var id in blockDef)
                                {
                                    BlockReference point = tr.GetObject(id, OpenMode.ForRead) as BlockReference;
                                    if (point != null && point.Name == "WheelStopPoint")
                                    {

                                        ObjectId att = point.AttributeCollection[0];
                                        //AttributeDefinition at = tr.GetObject(att, OpenMode.ForRead) as AttributeDefinition;
                                        AttributeReference ar = tr.GetObject(att, OpenMode.ForRead) as AttributeReference;
                                        int num = Int32.Parse(ar.TextString);
                                        double X = point.Position.X;
                                        double Y = point.Position.Y;
                                        double Z = point.Position.Z;
                                        string name = point.Name;
                                        table.Rows.Add(num, X, Y, Z, name);
                                    }
                                }
                            }
                            ToCSV(table, fbd.SelectedPath + "\\WheelStopPoint.csv");
                        }
                        else if(vm.WheelStopPoint == false && vm.ParkingPoint == true)
                        {
                            System.Data.DataTable table = createDataTable();
                            foreach (ObjectId blkid in ids)
                            {
                                if (blkid.IsValid == false)
                                {
                                    continue;
                                }
                                BlockReference blk = tr.GetObject(blkid, OpenMode.ForRead) as BlockReference;
                                if (blk == null)
                                {
                                    continue;
                                }
                                BlockTableRecord blockDef = tr.GetObject(blk.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
                                foreach (var id in blockDef)
                                {
                                    BlockReference point = tr.GetObject(id, OpenMode.ForRead) as BlockReference;
                                    if (point != null && point.Name == "ParkingPoint")
                                    {

                                        ObjectId att = point.AttributeCollection[0];
                                        //AttributeDefinition at = tr.GetObject(att, OpenMode.ForRead) as AttributeDefinition;
                                        AttributeReference ar = tr.GetObject(att, OpenMode.ForRead) as AttributeReference;
                                        int num = Int32.Parse(ar.TextString);
                                        double X = point.Position.X;
                                        double Y = point.Position.Y;
                                        double Z = point.Position.Z;
                                        string name = point.Name;
                                        table.Rows.Add(num, X, Y, Z, name);
                                    }
                                }
                            }
                            ToCSV(table, fbd.SelectedPath + "\\ParkingPoint.csv");
                        }
                        else if (vm.WheelStopPoint == true && vm.ParkingPoint == true && vm.Combine == false)
                        {
                            System.Data.DataTable table1 = createDataTable();
                            System.Data.DataTable table2 = createDataTable();
                            foreach (ObjectId blkid in ids)
                            {

                                if (blkid.IsValid == false)
                                {
                                    continue;
                                }
                                BlockReference blk = tr.GetObject(blkid, OpenMode.ForRead) as BlockReference;
                                if (blk == null)
                                {
                                    continue;
                                }
                                BlockTableRecord blockDef = tr.GetObject(blk.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
                                foreach (var id in blockDef)
                                {
                                    BlockReference point = tr.GetObject(id, OpenMode.ForRead) as BlockReference;
                                    if (point != null && point.Name == "WheelStopPoint")
                                    {

                                        ObjectId att = point.AttributeCollection[0];
                                        //AttributeDefinition at = tr.GetObject(att, OpenMode.ForRead) as AttributeDefinition;
                                        AttributeReference ar = tr.GetObject(att, OpenMode.ForRead) as AttributeReference;
                                        int num = Int32.Parse(ar.TextString);
                                        double X = point.Position.X;
                                        double Y = point.Position.Y;
                                        double Z = point.Position.Z;
                                        string name = point.Name;
                                        table1.Rows.Add(num, X, Y, Z, name);
                                    }
                                    if (point != null && point.Name == "ParkingPoint")
                                    {

                                        ObjectId att = point.AttributeCollection[0];
                                        //AttributeDefinition at = tr.GetObject(att, OpenMode.ForRead) as AttributeDefinition;
                                        AttributeReference ar = tr.GetObject(att, OpenMode.ForRead) as AttributeReference;
                                        int num = Int32.Parse(ar.TextString);
                                        double X = point.Position.X;
                                        double Y = point.Position.Y;
                                        double Z = point.Position.Z;
                                        string name = point.Name;
                                        table2.Rows.Add(num, X, Y, Z, name);
                                    }
                                }
                            }
                            ToCSV(table1, fbd.SelectedPath + "\\WheelStopPoint.csv");
                            ToCSV(table2, fbd.SelectedPath + "\\ParkingPoint.csv");
                        }
                    }
                    view.Close();
                }
            }
            
        }
        public static void ToCSV(System.Data.DataTable dtDataTable, string strFilePath)
        {
            StreamWriter sw = new StreamWriter(strFilePath, false);
            //headers    
            for (int i = 0; i < dtDataTable.Columns.Count; i++)
            {
                sw.Write(dtDataTable.Columns[i]);
                if (i < dtDataTable.Columns.Count - 1)
                {
                    sw.Write(",");
                }
            }
            sw.Write(sw.NewLine);
            foreach (DataRow dr in dtDataTable.Rows)
            {
                for (int i = 0; i < dtDataTable.Columns.Count; i++)
                {
                    if (!Convert.IsDBNull(dr[i]))
                    {
                        string value = dr[i].ToString();
                        if (value.Contains(','))
                        {
                            value = String.Format("\"{0}\"", value);
                            sw.Write(value);
                        }
                        else
                        {
                            sw.Write(dr[i].ToString());
                        }
                    }
                    if (i < dtDataTable.Columns.Count - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.Write(sw.NewLine);
            }
            sw.Close();
        }
        public static DataTable createDataTable()
        {
            DataTable table = new DataTable();
            //columns  
            table.Columns.Add("Number", typeof(int));
            table.Columns.Add("X", typeof(double));
            table.Columns.Add("Y", typeof(double));
            table.Columns.Add("Z", typeof(double));
            table.Columns.Add("Type", typeof(string));

            return table;
        }
    }
}
