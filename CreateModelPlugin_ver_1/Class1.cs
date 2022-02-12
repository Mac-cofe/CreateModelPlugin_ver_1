using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateModelPlugin_ver_1
{
    [TransactionAttribute(TransactionMode.Manual)]

    public class CreationModel : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            Level level1 = SelectLevel(doc, "Уровень 1");
            Level level2 = SelectLevel(doc, "Уровень 2");

            double width = UnitUtils.ConvertToInternalUnits(10000, UnitTypeId.Millimeters); // длина
            double depth = UnitUtils.ConvertToInternalUnits(5000, UnitTypeId.Millimeters);  // глубина

            double dx = width / 2;
            double dy = depth / 2;

            Transaction ts = new Transaction(doc, "Построение стен");
            ts.Start();
            {
                CreateWall(dx, dy, level1, level2, doc);
            }
            ts.Commit();

            return Result.Succeeded;
        }

        public Level SelectLevel(Document doc, string levelName)  // метод отбора уровня
        {
            List<Level> listLevels = new FilteredElementCollector(doc)  // фильтр по уровням
            .OfClass(typeof(Level))
            .OfType<Level>()
            .ToList();
            Level SelectLevel = listLevels
                            .Where(x => x.Name.Equals(levelName))       // фильтр по имени уровня
                            .OfType<Level>()
                            .FirstOrDefault();
            return SelectLevel;
        }

        public List<Wall> CreateWall(double dx, double dy, Level lowLevel, Level highLevel, Document doc) // метод создания стен
        {
            List<Wall> walls = new List<Wall>();                    // список стен
            List<XYZ> points = new List<XYZ>();                     // список точек
            for (int i = 0; i < 4; i++)
            {
                points.Add(new XYZ(-dx, -dy, 0));
                points.Add(new XYZ(dx, -dy, 0));
                points.Add(new XYZ(dx, dy, 0));
                points.Add(new XYZ(-dx, dy, 0));
                points.Add(new XYZ(-dx, -dy, 0));
                Line line = Line.CreateBound(points[i], points[i + 1]);   // строим линию
                Wall wall = Wall.Create(doc, line, lowLevel.Id, false);   // строим стену по линии
                wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(highLevel.Id);   // ограничение стены сверху
                walls.Add(wall);                                        // добавляем в список созданную стену
            }
            return walls;
        }

    }

}
