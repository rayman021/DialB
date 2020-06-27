using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace DialB
{
    static class DialAlgorithm
    {
        public static void Execute()
        {
            #region parameter settings
            string link = "road_link.csv";
            string node = "node.csv";
            string od = "demand.csv";
            string output = "output_obj.csv";
            string output_link_flow = "output_link_flow.csv";
            int total_iter = 50;

            #endregion


            Network.InitDataGMNS(link, node, od);
            

            StreamWriter sw = new StreamWriter(output, false);
            sw.WriteLine("obj_value,relative_gap");

            //List<int> topOrder = new List<int>();
            bool[] l = new bool[Network.AllLink.Count];
            for (int i = 0; i < Network.AllLink.Count; i++)
            {
                l[i] = true;
            }
            double[] f = new double[Network.AllLink.Count];
            for (int i = 0; i < Network.AllLink.Count; i++)
            {
                f[i] = 0.0;
                //LinkFlow[i] = 0.0;
            }
            Network.LinkToNode(l, Network.AllNode);
            Network.UpdateFlowCost();

            foreach (Bush b in Network.AllBush)
            {
                b.InitNode();
                b.InitBush();
                b.UpdateTopAndNode();
            }
            double value = 0.0;
            
            Network.UpdateFlowCost();

          
            value = Network.MainFunctionObjectiveValue();
            //sw.WriteLine(value.ToString());
            for (int i = 0; i < total_iter; i++)
            {
                foreach (Bush b in Network.AllBush)
                {
                    //Network.UpdateFlowCost();
                    b.UpdateBush();
                    b.UpdateTopAndNode();
                }
                for (int j = 0; j <10; j++)
                {
                    foreach (Bush b in Network.AllBush)
                    {
                        b.LongShortPath();
                        b.ChangeFlow();
                        Network.UpdateFlowCost();
                    }
                }
                double qr = 0.0;
                foreach (Bush b in Network.AllBush)
                {
                    foreach (OD nowOD in b.SubOD)
                    {
                        qr += nowOD.Flow * b.SubNode[nowOD.dPoint].MinPathCost;
                    }
                }
                double sumCost = 0.0;
                foreach (Link ll in Network.AllLink)
                {
                    sumCost += ll.Cost * ll.Flow;
                }
                value = 1 - qr / sumCost;
                double value_2;
                value_2 = Network.MainFunctionObjectiveValue();
                sw.WriteLine(value.ToString()+','+value_2.ToString());
            }
            sw.Close();

            #region output linkflow
            sw = new StreamWriter(output_link_flow);
            sw.WriteLine("link_id,link_flow");

            foreach(Link ll in Network.AllLink)
            {
                sw.WriteLine(ll.ID.ToString() + ',' + ll.Flow.ToString());
            }

            sw.Close();
            #endregion

            MessageBox.Show("finish!");
        }
    }
}
