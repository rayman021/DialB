using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DialB
{
    static class Network
    {
        public static List<Node> AllNode = new List<Node>();
        public static List<Link> AllLink = new List<Link>();
        public static List<OD> AllOD = new List<OD>();                  //od pair list
        public static List<Bush> AllBush = new List<Bush>();
        public static List<int> AlloPoint = new List<int>();            //origin point list
        public static List<List<int>> ODIndex = new List<List<int>>();  //od pair list for each origin
        public static double Step = 0.5;


        #region initialize data. finished
        public static void InitData(string link, string node, string od)
        {
            InitNode(node);
            InitLink(link);
            InitOD(od);
            InitBush();
        }

        public static void InitDataGMNS(string link, string node, string od)
        {
            InitNodeGMNS(node);
            InitLinkGMNS(link);
            InitODGMNS(od);
            InitBush();
        }

        private static void InitLink(string linkFilePath)
        {
            AllLink.Clear();
            StreamReader sr = File.OpenText(linkFilePath);
            string input;
            string[] data = null;

            while (sr.EndOfStream == false)
            {
                int oPointID, dPointID, oPointIndex, dPointIndex;


                input = sr.ReadLine();
                data = input.Split('\t');
                Link l = new Link();
                l.ID = Convert.ToInt32(data[0]);
                oPointID = Convert.ToInt32(data[1]);
                dPointID = Convert.ToInt32(data[2]);
                oPointIndex = AllNode.FindIndex(delegate(Node n) { return n.ID == oPointID; });
                dPointIndex = AllNode.FindIndex(delegate(Node n) { return n.ID == dPointID; });
                l.StartPoint = oPointIndex;
                l.EndPoint = dPointIndex;
                l.FreeFlowTime = Convert.ToDouble(data[3]);
                l.Capacity = Convert.ToDouble(data[4]);
                l.B = Convert.ToDouble(data[5]);
                l.Pow = Convert.ToDouble(data[6]);
                AllLink.Add(l);
            }
            sr.Close();
        }
        private static void InitOD(string odFilePath)
        {
            AllOD.Clear();
            AlloPoint.Clear();
            ODIndex.Clear();
            StreamReader sr = File.OpenText(odFilePath);
            string input;
            string[] data = null;
            int nowODIndex = 0;

            while (sr.EndOfStream == false)
            {
                input = sr.ReadLine();
                data = input.Split('\t');
                OD od = new OD();
                od.ID = Convert.ToInt32(data[0]);
                int oPointID, oPointIndex, dPointID, dPointIndex;
                oPointID = Convert.ToInt32(data[1]);
                dPointID = Convert.ToInt32(data[2]);
                oPointIndex = AllNode.FindIndex(delegate(Node n) { return n.ID == oPointID; });
                dPointIndex = AllNode.FindIndex(delegate(Node n) { return n.ID == dPointID; });
                od.oPoint = oPointIndex;
                od.dPoint = dPointIndex;
                od.Flow = Convert.ToDouble(data[3]);
                AllOD.Add(od);       

                int nowOPointIndex;
                nowOPointIndex = AlloPoint.FindIndex(delegate(int i) { return i == oPointIndex; });
                if (nowOPointIndex != -1)
                {
                    ODIndex[nowOPointIndex].Add(nowODIndex);
                }
                else
                {
                    AlloPoint.Add(oPointIndex);
                    List<int> newODIndex = new List<int>();
                    newODIndex.Add(nowODIndex);
                    ODIndex.Add(newODIndex);
                }

                nowODIndex++;
            }
            sr.Close();
        }
        private static void InitNode(string nodeFilePath)
        {
            AllNode.Clear();
            StreamReader sr = File.OpenText(nodeFilePath);

            String input;

            while (sr.EndOfStream == false)
            {
                input = sr.ReadLine();
                Node n = new Node();
                n.ID = Convert.ToInt32(input);
                AllNode.Add(n);
            }

            sr.Close();
        }

        private static void InitNodeGMNS(string nodeFilePath)
        {
            AllNode.Clear();

            CSVParser csv_file = new CSVParser(nodeFilePath);
            csv_file.Open();

            csv_file.ReadHeadTitle();
            while (!csv_file.IsEndOfStream())
            {
                csv_file.ReadDataByLine();

                Node n = new Node();
                csv_file.GetFieldValue("node_id", out n.ID);
                AllNode.Add(n);
            }

            csv_file.Close();         
        }
        private static void InitLinkGMNS(string linkFilePath)
        {
            AllLink.Clear();

            CSVParser csv_file = new CSVParser(linkFilePath);
            csv_file.Open();

            csv_file.ReadHeadTitle();
            while (!csv_file.IsEndOfStream())
            {
                csv_file.ReadDataByLine();


                Link l = new Link();
                int oPointID, dPointID;
                csv_file.GetFieldValue("road_link_id", out l.ID);
                csv_file.GetFieldValue("from_node_id", out oPointID);
                csv_file.GetFieldValue("to_node_id", out dPointID);
                csv_file.GetFieldValue("VDF_fftt1", out l.FreeFlowTime);
                csv_file.GetFieldValue("VDF_cap1", out l.Capacity);
                csv_file.GetFieldValue("VDF_alpha1", out l.B);
                csv_file.GetFieldValue("VDF_beta1", out l.Pow);

                l.StartPoint = AllNode.FindIndex(n => n.ID == oPointID);
                l.EndPoint = AllNode.FindIndex(n => n.ID == dPointID);

 
                AllLink.Add(l);
            }

            csv_file.Close();
        }
        private static void InitODGMNS(string odFilePath)
        {
            AllOD.Clear();
            AlloPoint.Clear();
            ODIndex.Clear();

            int nowODIndex = 0;

            CSVParser csv_file = new CSVParser(odFilePath);
            csv_file.Open();

            csv_file.ReadHeadTitle();
            while (!csv_file.IsEndOfStream())
            {
                csv_file.ReadDataByLine();

                OD od = new OD();
                od.ID = AllOD.Count;
                int oPointID, oPointIndex, dPointID, dPointIndex;

                csv_file.GetFieldValue("o", out oPointID);
                csv_file.GetFieldValue("d", out dPointID);

                oPointIndex = AllNode.FindIndex(n=> n.ID == oPointID);
                dPointIndex = AllNode.FindIndex(n => n.ID == dPointID);
                od.oPoint = oPointIndex;
                od.dPoint = dPointIndex;

                csv_file.GetFieldValue("value", out od.Flow);
                AllOD.Add(od);

                //==================//
                int nowOPointIndex;
                nowOPointIndex = AlloPoint.FindIndex(delegate (int i) { return i == oPointIndex; });
                if (nowOPointIndex != -1)
                {
                    ODIndex[nowOPointIndex].Add(nowODIndex);
                }
                else
                {
                    AlloPoint.Add(oPointIndex);
                    List<int> newODIndex = new List<int>();
                    newODIndex.Add(nowODIndex);
                    ODIndex.Add(newODIndex);
                }

                nowODIndex++;

            }

            csv_file.Close();
        }


        private static void InitBush()
        {
            AllBush.Clear();
            //foreach (int i in AlloPoint)
            //{
            //    Bush b = new Bush();
            //    b.oPointID = i;
            //    b.SubOD = ODIndex[i];
            //    AllBush.Add(b);
            //}
            for (int i = 0; i < AlloPoint.Count; i++)
            {
                Bush b = new Bush();
                b.oPointID = AlloPoint[i];
                b.SubOD.Clear();
                foreach (int j in ODIndex[i])
                {
                    b.SubOD.Add(AllOD[j]);
                }
                AllBush.Add(b);
            }
        }
        #endregion
        #region initialize relationship between link and node. finished
        public static void LinkToNode(bool[] mLink, List<Node> mNode)
        {
            foreach (Node n in mNode)
            {
                n.ForeLinkIndex.Clear();
                n.BackLinkIndex.Clear();
            }

            int i;
            for (i = 0; i < AllLink.Count; i++)
            {
                if (mLink[i] == true)
                {
                    int startPointIndex, endPointIndex;
                    //int startPointID, endPointID;
                    startPointIndex = AllLink[i].StartPoint;
                    //startPointIndex = AllNode.FindIndex(delegate(Node iii) { return iii.ID == startPointID; });
                    endPointIndex = AllLink[i].EndPoint;
                    //endPointIndex = AllNode.FindIndex(delegate(Node iii) { return iii.ID == endPointID; });
                    mNode[startPointIndex].ForeLinkIndex.Add(i);
                    mNode[endPointIndex].BackLinkIndex.Add(i);
                }
            }
        }
        #endregion   
        #region find toplogical order. finished
        public static void FindTopOrder(List<Node> mNode, List<int> topOrder, int oPointID)
        {
            topOrder.Clear();
            int totalPoint = mNode.Count;
            int[] degree = new int[totalPoint];
            List<int> checkList = new List<int>();
            int i;
            int totalLink;
            checkList.Clear();
            for (i = 0; i < totalPoint; i++)
                degree[i] = mNode[i].BackLinkIndex.Count;
            int oPointIndex;
            oPointIndex = oPointID;
            //oPointIndex = mNode.FindIndex(delegate(int iii) { return iii == oPointID; });
            checkList.Add(oPointIndex);

            while (checkList.Count > 0)
            {
                int nowPointIndex = checkList[0];
                checkList.RemoveAt(0);
                topOrder.Add(nowPointIndex);

                //int totalLink;
                totalLink = mNode[nowPointIndex].ForeLinkIndex.Count;

                //int i;
                for (i = 0; i < totalLink; i++)
                {
                    int newPointIndex, nowLinkIndex;
                    nowLinkIndex = mNode[nowPointIndex].ForeLinkIndex[i];
                    newPointIndex = AllLink[nowLinkIndex].EndPoint;
                    degree[newPointIndex]--;
                    if (degree[newPointIndex] == 0)
                        checkList.Add(newPointIndex);
                }
            }
        }
        #endregion

        #region update link flow and cost. finished
        public static void UpdateFlowCost()
        {
            int i, j;
            double[] f = new double[AllLink.Count];
            for (i = 0; i < AllLink.Count; i++)
            {
                f[i] = 0.0;
            }

            
            for (i = 0; i < AllBush.Count; i++)
            {
                for (j = 0; j < AllLink.Count; j++)
                {
                    f[j] += AllBush[i].LinkFlow[j];
                }
            }

            for (i = 0; i < AllLink.Count; i++)
            {
                AllLink[i].UpdateLinkFlow(f[i]);
                AllLink[i].UpdateLinkCost();
            }
        }
        #endregion

        #region two paths without common link
        public static void TwoPathWithoutCommonLink(List<int> path1, List<int> path2, List<int> linkIndex)
        {
            linkIndex.Clear();
            foreach (int link1 in path1)
            {
                linkIndex.Add(link1);
            }
            foreach (int link2 in path2)
            {
                int index;
                index = linkIndex.FindIndex(delegate(int iii) { return iii == link2; });
                if (index == -1)
                {
                    linkIndex.Add(link2);
                }
                else
                {
                    linkIndex.RemoveAt(index);                  
                }
            }
        }
        #endregion

        #region calculate sum of derivative costs of all links
        public static double CalSumCostD(List<int> linkIndex)
        {
            double sum = 0.0;
            foreach (int i in linkIndex)
            {
                sum += AllLink[i].CostD;
            }
            return sum;
        }
        #endregion

        public static double CalObjectiveValue(bool[] link)
        {
            double value = 0.0;

            int i;
            for (i = 0; i < AllLink.Count; i++)
            {
                if (link[i] == true)
                {
                    value += AllLink[i].CostI;
                }
            }
            return value;
        }

        public static double FindMinFlow(double[] flow, double odFlow, List<int> path)
        {
            double pathFlow = 0.0;
            if (path.Count > 0)
            {
                pathFlow = flow[path[0]]; 
                for (int i = 1; i < path.Count; i++)
                {
                    pathFlow = Math.Min(flow[path[i]], pathFlow);
                }
                //pathFlow = Math.Min(pathFlow, odFlow);
                for (int i = 0; i < path.Count; i++)
                {
                    flow[path[i]] -= pathFlow;
                }
            }
            return pathFlow;
        }

        public static double MainFunctionObjectiveValue()
        {
            double fov = 0.0;
            foreach (Link l in AllLink)
            {
                fov += l.CostI;
            }
            return fov;
        }

        public static void Dijsktra(int[] p, double[] cost, int oPoint, List<Node> subNode)
        {
            int i;
            double maxCost = 0.0;
            List<int> checkList = new List<int>();
            checkList.Clear();

            bool[] binlist = new bool[AllNode.Count];
            //1 initialize

            foreach (Link l in AllLink)
            {
                maxCost += l.Cost;
            }
            maxCost *= 10.0;
            maxCost = double.MaxValue;
            for (i = 0; i < AllNode.Count; i++)
            {
                p[i] = -1;
                cost[i] = maxCost;
            }
            cost[oPoint] = 0;
            binlist[oPoint] = true;
            checkList.Add(oPoint);
            //2 iteration
            while (checkList.Count > 0)
            {
                int nowPoint = checkList[0];
                int linkCount = subNode[nowPoint].ForeLinkIndex.Count;
                int nowLink;
                for (i = 0; i < linkCount; i++)
                {
                    nowLink = subNode[nowPoint].ForeLinkIndex[i];
                    int newPoint = AllLink[nowLink].EndPoint;
                    if (cost[nowPoint] + AllLink[nowLink].Cost < cost[newPoint])
                    {
                        cost[newPoint] = cost[nowPoint] + AllLink[nowLink].Cost;
                        p[newPoint] = nowLink;

                        if (!binlist[newPoint])
                        {
                            binlist[newPoint] = true;
                            checkList.Add(newPoint);
                        }
                    }                    
                }
                checkList.RemoveAt(0);
                binlist[nowPoint] = false;
            }
        }
    }
}