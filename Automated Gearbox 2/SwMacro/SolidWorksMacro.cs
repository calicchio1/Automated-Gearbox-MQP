using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System.Runtime.InteropServices;
using System;
using System.Diagnostics;

namespace Automated_Gearbox_2.csproj
{
    public partial class SolidWorksMacro
    {


        public void Main()
        {


            ModelDoc2 swDoc;
            PartDoc swPart = null;
            DrawingDoc swDrawing = null;
            AssemblyDoc swAssembly = null;

            bool boolstatus = false;

            int longstatus = 0;
            int longwarnings = 0;

            string FileLocation = "E:\\My Douments\\MQP\\SOLIDWORKS\\Gear Creation script\\"; //directory for all parts created and modified in this code (Templates should be here)
            string Gear_Template = "Spur gear - Metric Template.sldprt";   //filename of the spur gear template
            string Housing = "GEAR_HOUSING_MACRO.sldprt"; //filename for gearbox housing
            string ASSEMBLY = "GEARBOX_ASSEMBLY_MACRO.sldasm"; //filename for Assembly file
            string SHAFT_TEMPLATE = "SHAFT_TEMPLATE.sldprt";
            int i; //number of gears to create (replace with input data later)


            //***************************CREATE GEARS***********************************

            int num_gears = 5; //number of gears in assembly to generate
            double[][] Gear_info = new double[num_gears][]; //array of arrays(jagged) to store data about each gear

            //raw gear data           {#teeth, width,shaft,locked to shaft, other gear to mesh with(255 indicates no output),distance from }
                                     //    0  1  2  3  4    5
            Gear_info[0] = new double[] { 10, 4, 0, 1, 1   ,0.000 }; // #teeth, width mm, which shaft to connect to, if locked on shaft, distance between shafts
            Gear_info[1] = new double[] { 40, 4, 1, 0, 2   ,0.000 };
            Gear_info[2] = new double[] { 20, 4, 2, 1, 255 ,0.000 };
            Gear_info[3] = new double[] { 40, 4, 2, 1, 4   ,0.004 };
            Gear_info[4] = new double[] { 20, 4, 1, 1, 255, 0.004 };            
                      
            //shaft info
            double Shaft_Diameter = 1; //diameter in mm (also used for bore dia. in gears)
            int num_shafts = 3; 
            double Shaft_length = 2 * Gear_info[0][1]; //length of shaft, make slightly larger than width of gear for now
            double Modulus = .25; //gear modulus for all gears


            double[] shaft_spacing = new double[] { 4, 5, 8 }; //temporarily use this for the shaft spacing



            for (i = 0; i < num_gears; i++) //create each gear based off the template
            {

                swDoc = ((ModelDoc2)(swApp.OpenDoc6(FileLocation + Gear_Template, 1, 0, "", ref longstatus, ref longwarnings)));
                double NUM_TEETH = Gear_info[i][0];
                Double WIDTH = Gear_info[i][1];//      gear depth in mm
                double SHOW_TEETH = NUM_TEETH;
                //change sketch dimmensions in the reference sketch: "HoldingSke"
                ((Dimension)(swDoc.Parameter("Show_teeth@HoldingSke"))).SystemValue = SHOW_TEETH * (3.14159 / 180); //have to convert degrees to radians, num_teeth is saved as angle measurment in the sketch
                ((Dimension)(swDoc.Parameter("Num_teeth@HoldingSke"))).SystemValue = NUM_TEETH * (3.14159 / 180); //display all the teeth
                ((Dimension)(swDoc.Parameter("Width@HoldingSke"))).SystemValue = WIDTH / 1000;
                ((Dimension)(swDoc.Parameter("Bore@HoldingSke"))).SystemValue = Shaft_Diameter / 1000;
                ((Dimension)(swDoc.Parameter("Module@HoldingSke"))).SystemValue = Modulus / 1000;
                boolstatus = swDoc.EditRebuild3(); //rebuild the document

                longstatus = swDoc.SaveAs3(FileLocation + "GEAR_MACRO" + i + ".sldprt", 0, 2); //save gear as gear number i in a new folder called MACRO_PARTS

            }

            //***************************CREATE SHAFTS***********************************


            for (i = 0; i < num_shafts; i++)
            {


                swDoc = ((ModelDoc2)(swApp.OpenDoc6(FileLocation + SHAFT_TEMPLATE, 1, 0, "", ref longstatus, ref longwarnings)));

                //change sketch dimmensions
                ((Dimension)(swDoc.Parameter("Diameter@Sketch1"))).SystemValue = Shaft_Diameter / 1000;
                ((Dimension)(swDoc.Parameter("Shaft Length@Sketch1"))).SystemValue = Shaft_length / 1000;
                boolstatus = swDoc.EditRebuild3(); //rebuild the document

                longstatus = swDoc.SaveAs3(FileLocation + "SHAFT_MACRO" + i + ".sldprt", 0, 2); //save gear as gear number i in a new folder called MACRO_PARTS
                //code to create shafts of set diameters or something
            }

            
            

            //***************************CREATE ASSEMBLY***********************************
            //Open All components to be added
            
            for (i = 0; i < num_gears; i++)
            {
                swDoc = ((ModelDoc2)(swApp.OpenDoc6(FileLocation + "GEAR_MACRO" + i + ".sldprt", 1, 0, "", ref longstatus, ref longwarnings)));
                swDoc = ((ModelDoc2)(swApp.OpenDoc6(FileLocation + "SHAFT_MACRO" + i + ".sldprt", 1, 0, "", ref longstatus, ref longwarnings)));
            }
            
 
            //Open the assembly
            swDoc = ((ModelDoc2)(swApp.OpenDoc6(FileLocation + "Assembly_Template.SLDASM", 2, 0, "", ref longstatus, ref longwarnings)));
            swDoc = ((ModelDoc2)(swApp.ActiveDoc));
            swApp.ActivateDoc2("Assembly_Template.SLDASM", false, ref longstatus);
            swAssembly = ((AssemblyDoc)(swDoc));

            //add components to SW assembly
            //****** Currently Parts must already be opened seperately to add to assmebly *******

            

            //***************************SHAFT MATES***********************************
            //names of edges and faces in the template:  Front Edge ( circle along front)
            //                                           Surface (cylinder surface of rod)

            //make face coincident with corresponding hole on housing

            Mate2 myMate = null;
            
            //insert shaft
            //insert point for mate
            //align along axis
            //mate oring to point

            double[][] Shaft_coord = new double[num_shafts][]; //array of arrays(jagged) to store data about each gear
            
            double x; double y;  double z;
            //shaft coords                    x,    y, z
            Shaft_coord[0] = new double[] {.000, .000, .000}; 
            Shaft_coord[1] = new double[] {.000, .000, .008};
            Shaft_coord[2] = new double[] {.000, .000, .018};
            //Shaft_coord[3] = new double[] {.019, .000, 0};



            



            

            for (i = 0; i < num_shafts; i++)
            {
                
                boolstatus = swAssembly.AddComponent(FileLocation + "SHAFT_Macro" + i + ".sldprt", 0, 0, 0);
                if (i == 0)
                {
                    swDoc.ClearSelection2(true);
                    boolstatus = swDoc.Extension.SelectByID2("SHAFT_MACRO" + i + "-1@Assembly_Template", "COMPONENT", 0, 0, 0, false, 0, null, 0);
                    swAssembly.UnfixComponent();
                }



                x = Shaft_coord[i][0]; 
                y = Shaft_coord[i][1]; 
                z = Shaft_coord[i][2];
            

                boolstatus = swDoc.Extension.SelectByID2("Shaft coords sketch", "SKETCH", 0, 0, 0, false, 0, null, 0);
                swDoc.EditSketch();
                swDoc.ClearSelection2(true);
                SketchPoint skPoint = null;
                skPoint = ((SketchPoint)(swDoc.SketchManager.CreatePoint(x, y, z))); //create a point

               


                //have to make point fixed!

                //make origin coincident to its sketch point
                boolstatus = swDoc.Extension.SelectByID2("Point" + (i + 11) + "@Shaft coords sketch", "EXTSKETCHPOINT", x, y, z, true, 1, null, 0);
                boolstatus = swDoc.Extension.SelectByID2("Point1@Origin@SHAFT_MACRO" + i + "-1@Assembly_Template", "EXTSKETCHPOINT", 0, 0, 0, true, 1, null, 0);
                myMate = null;
                swAssembly = ((AssemblyDoc)(swDoc));
                myMate = ((Mate2)(swAssembly.AddMate5(0, 0, false, 0, 0, 0, 0, 0, 0, 0, 0, false, false, 0, out longstatus)));
                swDoc.ClearSelection2(true);
                swDoc.EditRebuild3();
                
                //parallel to axis
                boolstatus = swDoc.Extension.SelectByID2("Center Axis@SHAFT_MACRO" + i + "-1@Assembly_Template", "AXIS", 0, 0, 0, false, 0, null, 0);
                boolstatus = swDoc.Extension.SelectByID2("Line1@Reference Direction", "EXTSKETCHSEGMENT", 0, 0, 0.0021382559642120218, true, 1, null, 0);
                myMate = null;
                swAssembly = ((AssemblyDoc)(swDoc));
                myMate = ((Mate2)(swAssembly.AddMate5(3, 1, false, 0.0029999999999767309, 0.001, 0.001, 0.001, 0.001, 0, 0.5235987755983, 0.5235987755983, false, false, 0, out longstatus)));
                swDoc.ClearSelection2(true);
                swDoc.EditRebuild3();
                                
            }

        


            //***************************GEAR MATES***********************************
            string face_sel; //holds string with name of the face to select

            for (i = 0; i < num_gears; i++)
            {

                Double DistanceMate = Gear_info[i][5]; //      gear loacation on shaft 
                boolstatus = swAssembly.AddComponent(FileLocation + "GEAR_MACRO" + i + ".sldprt", -0.5, 0, 0);

                //Distance mate from edge of shaft

                boolstatus = swDoc.Extension.SelectByID2("EDGE@GEAR_MACRO" + i + "-1@Assembly_Template", "PLANE", 0, 0, 0, true, 0, null, 0); //face plane of gear
                boolstatus = swDoc.Extension.SelectByID2("Right@SHAFT_MACRO" + Gear_info[i][2] + "-1@Assembly_Template", "PLANE", 0, 0, 0, true, 0, null, 0); //shaft hole in housing

                myMate = (Mate2)swAssembly.AddMate5(5, 1, false, DistanceMate, DistanceMate, DistanceMate, 0, 0, 0, 0, 0, false, false, 0, out longstatus);

                swDoc.ClearSelection2(true);

            
        
                //concentric to shaft
                boolstatus = swDoc.Extension.SelectByID2("Central Axis@GEAR_MACRO" + i + "-1@Assembly_Template", "AXIS", 0, 0, 0, true, 0, null, 0); //axis of gear
                boolstatus = swDoc.Extension.SelectByID2("Center Axis@SHAFT_MACRO" + Gear_info[i][2] + "-1@Assembly_Template", "AXIS", 0, 0, 0, true, 0, null, 0); //axis of shaft
                myMate = (Mate2)swAssembly.AddMate4((int)swMateType_e.swMateCOINCIDENT, (int)swMateAlign_e.swMateAlignALIGNED, false, 0, 0, 0, 0, 0, 0, 0, 0, false, false, out longstatus);
                swDoc.ClearSelection2(true);


                //lock to shaft
                if (Gear_info[i][3] == 1) //if gear locked to shaft (make two planes coincident)
                {
                    boolstatus = swDoc.Extension.SelectByID2("Plane2@GEAR_MACRO" + i + "-1@Assembly_Template", "PLANE", 0, 0, 0, true, 0, null, 0); //face plane of shaft
                    boolstatus = swDoc.Extension.SelectByID2("Front@SHAFT_MACRO" + Gear_info[i][2] + "-1@Assembly_Template", "PLANE", 0, 0, 0, true, 0, null, 0); //shaft hole in housing
                    myMate = (Mate2)swAssembly.AddMate4((int)swMateType_e.swMateCOINCIDENT, (int)swMateAlign_e.swMateAlignALIGNED, false, 0, 0, 0, 0, 0, 0, 0, 0, false, false, out longstatus);
                    swDoc.ClearSelection2(true);
                    swDoc.EditRebuild3();
                }


         
                
            }
           
            //gear mate
            for (i = 0; i < (num_gears); i++)
            {

                if (Gear_info[i][4] != 255) //if -1 that means do not make a gear mate (don't make gear mate for last gear sinc it is output, no gear to mesh to!!!!)
                {
                    boolstatus = swDoc.Extension.SelectByID2("Central Axis@GEAR_MACRO" + i + "-1@Assembly_Template", "AXIS", 0, 0, 0, true, 0, null, 0); //axis of gear
                    boolstatus = swDoc.Extension.SelectByID2("Central Axis@GEAR_MACRO" + Gear_info[i][4] + "-1@Assembly_Template", "AXIS", 0, 0, 0, true, 0, null, 0); //axis of gear
                    myMate = (Mate2)swAssembly.AddMate4((int)swMateType_e.swMateGEAR, (int)swMateAlign_e.swMateAlignALIGNED, false, 0, 0, 0, Gear_info[i][0], Gear_info[i + 1][0], 0, 0, 0, false, false, out longstatus); //gear mate, specify num. and denom. of gear ratio as #teeth on gear
                    swDoc.ClearSelection2(true);
                    swDoc.EditRebuild3();
                }
            }

            


            /*


                   //     boolstatus = swDoc.Extension.SelectByID2("", "FACE", -0.00455748224163699, 0.0037205844126901866, 0.004682722786242266, false, 0, null, 0);
                 //       boolstatus = swDoc.Extension.SelectByID2("", "FACE", 0.0018884588073433406, 0.00336302382845588, -0.00060759657679909651, true, 0, null, 1);
                 //       swDoc.ClearSelection2(true);
      
                        boolstatus = swDoc.Extension.SelectByID2("Plane3@30 tooth gear-1@Assem4", "PLANE", 0, 0, 0, true, 0, null, 0);
                        boolstatus = swDoc.Extension.SelectByID2("Plane3@50 tooth gear-1@Assem4", "PLANE", 0, 0, 0, true, 0, null, 0);
                        //Mate2 myMate = null;
                //        swAssembly = ((AssemblyDoc)(swDoc));
             
                        //add mate to two parts (by selection)
                    //    myMate = (Mate2)swAssembly.AddMate4((int)swMateType_e.swMateCOINCIDENT, (int)swMateAlign_e.swMateAlignALIGNED, false, 0, 0, 0, 0, 0, 0, 0, 0, false, false, out longstatus);
                  //      swDoc.ClearSelection2(true);


                        boolstatus = swDoc.Extension.SelectByID2("Bore of@50 tooth gear-1@Assem4", "PLANE", 0, 0, 0, true, 0, null, 0);


                        swDoc.EditRebuild3();
               */

        }


        public SldWorks swApp;
    }
}


