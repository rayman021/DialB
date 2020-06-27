using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DialB
{
    #region DataStruct finished
    class Node
    {
        public int ID;
        public List<int> ForeLinkIndex = new List<int>();
        public List<int> BackLinkIndex = new List<int>();
        //public int TopologicalOrder;
        public List<int> MinPath = new List<int>();
        public List<int> MaxPath = new List<int>();
        public double MinPathCost, MaxPathCost;
        public double MinPathFlow, MaxPathFlow;

        #region OB
        public double V, S;
        public int LCN;
        public int BasedLinkIndex;
        #endregion
    }
    class Link
    {
        public int ID;
        public int StartPoint;
        public int EndPoint;
        public double FreeFlowTime;
        public double Capacity;
        public double B;
        public double Pow;
        public double Flow;
        public double Cost;
        public double CostD;
        public double CostI;

        public void UpdateLinkCost()
        {
            Cost = FreeFlowTime * (1 + B * Math.Pow(Flow / Capacity, Pow));
            CostD = FreeFlowTime * B * Pow / Capacity * (Math.Pow(Flow / Capacity, Pow - 1));
            CostI = Flow * FreeFlowTime * (1 + B / (Pow + 1) * Math.Pow(Flow / Capacity, Pow));
        }
        public void UpdateLinkFlow(double f)
        {
            Flow = f;
        }
    }
    class OD
    {
        public int ID;
        public int oPoint, dPoint;
        public double Flow;
    }
    #endregion
    class Bush
    {
        public int oPointID;

        public bool[] SubLink = new bool[Network.AllLink.Count];
        public bool[] UsedLink = new bool[Network.AllLink.Count];
        public double[] LinkFlow = new double[Network.AllLink.Count];
        public double[] LinkAlpha = new double[Network.AllLink.Count];
        private double[] LinkV = new double[Network.AllLink.Count];
        private double[] LinkS = new double[Network.AllLink.Count];

        public List<Node> SubNode = new List<Node>();
        private List<int> TopologicalOrder = new List<int>();
        public List<OD> SubOD = new List<OD>();

        public double ObjectiveValue;
        public bool IsNewSubNetTheSame = false;

        #region initial
        #region initial nodes. finished
        public void InitNode()
        {
            SubNode.Clear();
            int i;
            for (i = 0; i < Network.AllNode.Count; i++)
            {
                Node n = new Node();
                n.ID = Network.AllNode[i].ID;
                n.MaxPath.Clear();
                n.MinPath.Clear();
                n.ForeLinkIndex.Clear();
                n.BackLinkIndex.Clear();
                SubNode.Add(n);
            }
        }
        #endregion
        #region initial bush. finished
        public void InitBush()
        {
            int[] forePoint = new int[SubNode.Count];
            double[] cost = new double[SubNode.Count];
            int i;
            for (i = 0; i < SubLink.Count(); i++)
            {
                SubLink[i] = false;
            }
            //1 calculate shortest path
            Network.Dijsktra(forePoint, cost, oPointID, Network.AllNode);
            //2 initialize link 
            for (i = 0; i < Network.AllNode.Count; i++)
            {
                int link;
                link = forePoint[i];
                if (link != -1)
                {
                    SubLink[link]=true;
                }
            }
            //3 calculate assignment
            for (i = 0; i < SubOD.Count; i++)
            {
                int dPoint;
                double odFlow;
                dPoint = SubOD[i].dPoint;
                odFlow = SubOD[i].Flow;

                int nowPoint, linkIndex;
                nowPoint = dPoint;

                while (nowPoint != oPointID)
                {
                    linkIndex = forePoint[nowPoint];
                    //update link flow
                    LinkFlow[linkIndex] += odFlow;
                    //update points
                    nowPoint = Network.AllLink[linkIndex].StartPoint;
                }
            }
        }
        #endregion
        #endregion

        public void UpdateBush()
        {
            IsNewSubNetTheSame = true;

            int i;
            double[] cost = new double[Network.AllNode.Count];
            double[] cheap = new double[Network.AllNode.Count];

            bool[] isShort = new bool[Network.AllLink.Count];
            int[] shortPath = new int[Network.AllNode.Count];

            double maxCost = double.MaxValue;

            for (i = 0; i < Network.AllNode.Count; i++)
            {
                cost[i] = 0.0;
                cheap[i] = maxCost;
                shortPath[i] = -1;
            }
            cost[TopologicalOrder[0]] = 0;
            cheap[TopologicalOrder[0]] = 0;
            //find longest path
            for (i = 0; i < TopologicalOrder.Count; i++)
            {
                int nowPoint;
                nowPoint = TopologicalOrder[i];
                int totalLink;
                totalLink = SubNode[nowPoint].ForeLinkIndex.Count;
                for (int j = 0; j < totalLink; j++)
                {
                    int newPoint;
                    int nowLink;
                    nowLink = SubNode[nowPoint].ForeLinkIndex[j];
                    newPoint = Network.AllLink[nowLink].EndPoint;
                    if (cost[nowPoint] + Network.AllLink[nowLink].Cost > cost[newPoint])
                    {
                        cost[newPoint] = cost[nowPoint] + Network.AllLink[nowLink].Cost;
                        //forePoint[newPoint] = nowPoint;
                    }
                    if (cheap[nowPoint] + Network.AllLink[nowLink].Cost < cheap[newPoint])
                    {
                        cheap[newPoint] = cheap[nowPoint] + Network.AllLink[nowLink].Cost;
                        //isShort[nowLink] = true;//*************************
                        shortPath[newPoint] = nowLink;
                    }
                }
            }
            for (i = 0; i < Network.AllNode.Count; i++)
            {
                if (shortPath[i] != -1)
                    isShort[shortPath[i]] = true;
            }
            //update sub-network
            for (i = 0; i < Network.AllLink.Count; i++)
            {
                if (SubLink[i] == true)
                {
                    if (LinkFlow[i] == 0.0)
                    {
                        SubLink[i] = false;
                    }
                }
                if (isShort[i] == true)
                {
                    SubLink[i] = true;
                }

                int sPoint, ePoint;
                sPoint = Network.AllLink[i].StartPoint;
                ePoint = Network.AllLink[i].EndPoint;
                if (cost[sPoint] + Network.AllLink[i].Cost < cost[ePoint])
                {
                    SubLink[i] = true;
                    IsNewSubNetTheSame = false;
                }
            }
            
        }
        public void UpdateBush(int k)
        {
            IsNewSubNetTheSame = true;

            int i;
            double[] cost = new double[Network.AllNode.Count];
            double[] cheap = new double[Network.AllNode.Count];

            bool[] isShort = new bool[Network.AllLink.Count];
            int[] shortPath = new int[Network.AllNode.Count];

            double maxCost = double.MaxValue;

            for (i = 0; i < Network.AllNode.Count; i++)
            {
                cost[i] = 0.0;
                cheap[i] = maxCost;
                shortPath[i] = -1;
            }
            cost[TopologicalOrder[0]] = 0;
            cheap[TopologicalOrder[0]] = 0;


            //1 find shortest path
            for (i = 0; i < TopologicalOrder.Count; i++)
            {
                int nowPoint;
                nowPoint = TopologicalOrder[i];
                int totalLink;
                totalLink = SubNode[nowPoint].ForeLinkIndex.Count;
                for (int j = 0; j < totalLink; j++)
                {
                    int newPoint;
                    int nowLink;
                    nowLink = SubNode[nowPoint].ForeLinkIndex[j];
                    newPoint = Network.AllLink[nowLink].EndPoint;
                    if (cheap[nowPoint] + Network.AllLink[nowLink].Cost < cheap[newPoint])
                    {
                        cheap[newPoint] = cheap[nowPoint] + Network.AllLink[nowLink].Cost;
                        shortPath[newPoint] = nowLink;
                    }
                }
            }
            for (i = 0; i < Network.AllNode.Count; i++)
            {
                if (shortPath[i] != -1)
                    isShort[shortPath[i]] = true;
            }
            //2 delete zero flow links
            for (i = 0; i < Network.AllLink.Count; i++)
            {
                if (SubLink[i] == true)
                {
                    if (LinkFlow[i] == 0.0)
                    {
                        SubLink[i] = false;
                    }
                }
                if (isShort[i] == true)
                    SubLink[i] = true;
            }

            //if (k == 11 && oPointID == 19)
            //{
            //    StreamWriter sw = new StreamWriter(@"d:\linkflow.txt", false);
            //    foreach (double f in LinkFlow)
            //    {
            //        sw.WriteLine(f);
            //    }
            //    sw.Close();
            //}
            //3 construct new network
            UpdateTopAndNode();

            //4 find longest path
            for (i = 0; i < TopologicalOrder.Count; i++)
            {
                int nowPoint;
                nowPoint = TopologicalOrder[i];
                int totalLink;
                totalLink = SubNode[nowPoint].ForeLinkIndex.Count;
                for (int j = 0; j < totalLink; j++)
                {

                    int newPoint;
                    int nowLink;
                    nowLink = SubNode[nowPoint].ForeLinkIndex[j];
                    newPoint = Network.AllLink[nowLink].EndPoint;
                    if (cost[nowPoint] + Network.AllLink[nowLink].Cost > cost[newPoint])
                    {
                        cost[newPoint] = cost[nowPoint] + Network.AllLink[nowLink].Cost;
                        //forePoint[newPoint] = nowPoint;
                    }
                }
            }
            //5 update sub-network
            for (i = 0; i < Network.AllLink.Count; i++)
            {
                int sPoint, ePoint;
                sPoint = Network.AllLink[i].StartPoint;
                ePoint = Network.AllLink[i].EndPoint;
                if (cost[sPoint] + Network.AllLink[i].Cost < cost[ePoint])
                {
                    SubLink[i] = true;
                    //IsNewSubNetTheSame = false;
                }
            }

        }
        #region update node information and topologic order
        public void UpdateTopAndNode()
        {
            LinkToSubNode();
            UpdateTopOrder();
        }
        #region Update top order of sub-network
        private void UpdateTopOrder()
        {
            Network.FindTopOrder(SubNode, TopologicalOrder, oPointID);
        }
        #endregion

        #region link to node
        private void LinkToSubNode()
        {
            Network.LinkToNode(SubLink, SubNode);
        }
        #endregion
        #endregion
        #region find used link
        private void FindUsedLink()
        {
            int i, j;
            //1 initialize
            for (i = 0; i < Network.AllLink.Count; i++)
            {
                UsedLink[i] = false;
            }

            //2 find used link
            for (i = 0; i < TopologicalOrder.Count; i++)
            {
                int totalLink;
                int nowLink;
                totalLink = SubNode[TopologicalOrder[i]].BackLinkIndex.Count;
                //for (j = 0; j < totalLink; j++)
                //{
                //    nowLink = SubNode[TopologicalOrder[i]].BackLinkIndex[j];
                //    if (LinkFlow[nowLink] > 0.0)
                //        UsedLink[nowLink] = true;
                //}
                if (totalLink > 0)
                {
                    double flow = 0.0;
                    for (j = 0; j < totalLink; j++)
                    {
                        nowLink = SubNode[TopologicalOrder[i]].BackLinkIndex[j];
                        flow += LinkFlow[nowLink];
                    }
                    if (flow == 0.0)
                    {
                        nowLink = SubNode[TopologicalOrder[i]].BackLinkIndex[0];
                        UsedLink[nowLink] = true;
                    }
                    else
                    {
                        for (j = 0; j < totalLink; j++)
                        {
                            nowLink = SubNode[TopologicalOrder[i]].BackLinkIndex[j];
                            if (LinkFlow[nowLink] > 0.0)
                                UsedLink[nowLink] = true;
                        }
                    }
                }
            }
        }
        #endregion
        #region find longest/shortest path
        private void FindPath(int[] fore, List<int> path, int oPoint, int dPoint)
        {
            path.Clear();
            int nowPoint, forePointIndex;
            nowPoint = dPoint;

            while (nowPoint != oPointID)
            {
                //forePointIndex = fore[nowPoint];
                //找路段
                int link;
                //link = Network.AllLink.FindIndex(delegate(Link l) { return l.StartPoint == forePointIndex && l.EndPoint == nowPoint; });
                link = fore[nowPoint];
                path.Add(link);
                //更新节点
                forePointIndex = Network.AllLink[link].StartPoint;
                nowPoint = forePointIndex;
            }
        }
        public void LongShortPath()
        {
            //FindUsedLink();
            foreach (Node n in SubNode)
            {
                n.MaxPathCost = 0.0;
                n.MaxPath.Clear();
                n.MinPathCost = 0.0;
                n.MinPath.Clear();
            }
            int i;
            int[] shortForePoint = new int[SubNode.Count];
            int[] longForePoint = new int[SubNode.Count];
            for (i = 0; i < SubNode.Count; i++)
            {
                shortForePoint[i] = -1;
                longForePoint[i] = -1;
            }

            double maxCost = 0.0;
            //for (i = 0; i < Network.AllLink.Count; i++)
            //{
            //    if (SubLink[i] == true)
            //    {
            //        maxCost += Network.AllLink[i].Cost;
            //    }
            //}
            maxCost=double.MaxValue;
            for (i = 0; i < SubNode.Count; i++)
            {
                SubNode[i].MinPathCost = maxCost;
                SubNode[i].MaxPathCost = 0.0;
            }
            SubNode[oPointID].MinPathCost = 0.0;

            for (i = 0; i < TopologicalOrder.Count; i++)
            {
                int nowPoint;
                nowPoint = TopologicalOrder[i];
                int totalLink;
                totalLink = SubNode[nowPoint].ForeLinkIndex.Count;
                for (int j = 0; j < totalLink; j++)
                {
                    int nowLink;
                    nowLink = SubNode[nowPoint].ForeLinkIndex[j];
                    int newPoint;
                    newPoint = Network.AllLink[nowLink].EndPoint;
                    if (SubNode[nowPoint].MinPathCost + Network.AllLink[nowLink].Cost < SubNode[newPoint].MinPathCost)
                    {
                        SubNode[newPoint].MinPathCost = SubNode[nowPoint].MinPathCost + Network.AllLink[nowLink].Cost;
                        shortForePoint[newPoint] = nowLink;
                        //UsedLink[nowLink] = true;
                    }

                    if (SubNode[nowPoint].MaxPathCost + Network.AllLink[nowLink].Cost > SubNode[newPoint].MaxPathCost)
                    {
                        SubNode[newPoint].MaxPathCost = SubNode[nowPoint].MaxPathCost + Network.AllLink[nowLink].Cost;
                        longForePoint[newPoint] = nowLink;
                    }
                  
                }
            } 
           

            for (i = 0; i < SubNode.Count; i++)
            {
                if (i != oPointID)
                {
                    FindPath(longForePoint, SubNode[i].MaxPath, oPointID, i);
                    FindPath(shortForePoint, SubNode[i].MinPath, oPointID, i);
                }
            }
        }
        #endregion

        public void UpdateObjectiveValue()
        {
            ObjectiveValue = Network.CalObjectiveValue(SubLink);
        }

        /// <summary>
        /// update shortest path
        /// </summary>
        public void ChangeFlow()
        {
            double[] tempLinkFlow = new double[Network.AllLink.Count];
            int i;
            for (i = 0; i < Network.AllLink.Count; i++)
            {
                tempLinkFlow[i] = LinkFlow[i];
            }
           
            for (i = 0; i < TopologicalOrder.Count; i++)
            {
                int nowPoint = TopologicalOrder[TopologicalOrder.Count - i - 1];
                int index;
                SubNode[nowPoint].MaxPathFlow = Network.FindMinFlow(tempLinkFlow, 0.0, SubNode[nowPoint].MaxPath);
            }

           
            //update flow
            for (i = 0; i < SubOD.Count; i++)
            {
                //find shortest/longest path
                int oPoint, dPoint;
                oPoint = oPointID;
                dPoint = SubOD[i].dPoint;
                if (SubNode[dPoint].MaxPathFlow > 0.0)
                {
                    List<int> shortPath = SubNode[dPoint].MinPath;
                    List<int> longPath = SubNode[dPoint].MaxPath;
                    double diffCost, sumCostD;
                    diffCost = SubNode[dPoint].MaxPathCost - SubNode[dPoint].MinPathCost;
                    List<int> linkIndex = new List<int>();
                    Network.TwoPathWithoutCommonLink(shortPath, longPath, linkIndex);
                    if (linkIndex.Count != 0)
                    {
                        sumCostD = Network.CalSumCostD(linkIndex);
                        double flow;
                        flow = Math.Min(Network.Step * diffCost / sumCostD, SubNode[dPoint].MaxPathFlow);
                        
                        //update flow
                        foreach (int link in shortPath)
                        {
                            LinkFlow[link] += flow;
                        }
                        foreach (int link in longPath)
                        {
                            LinkFlow[link] -= flow;
                        }
                    }
                }
            }
        }




        #region OB/unused

        public void UpdateNodeInfo()
        {
            SubNode[TopologicalOrder[0]].V = 0.0;
            SubNode[TopologicalOrder[0]].S = 0.0;

            for (int i = 0; i < Network.AllLink.Count; i++)
            {
                LinkV[i] = 0.0;
                LinkS[i] = 0.0;
            }

            for (int i = 0; i < TopologicalOrder.Count; i++)
            {
                int totalLink = SubNode[TopologicalOrder[i]].ForeLinkIndex.Count;
                //fore link
                for (int j = 0; j < totalLink; j++)
                {
                    int nowLink, nowPoint;
                    nowLink = SubNode[TopologicalOrder[i]].ForeLinkIndex[j];
                    nowPoint = Network.AllLink[nowLink].EndPoint;
                    double linkCost = Network.AllLink[nowLink].Cost;
                    double linkCostD = Network.AllLink[nowLink].CostD;
                    LinkV[i] = SubNode[TopologicalOrder[i]].V + linkCost;
                    LinkS[i] = SubNode[TopologicalOrder[i]].S + linkCostD;
                }
                //back link
                totalLink = SubNode[TopologicalOrder[i]].BackLinkIndex.Count;
                for (int j = 0; j < totalLink; j++)
                {
                    int nowLink;
                    nowLink = SubNode[TopologicalOrder[i]].BackLinkIndex[j];
                    SubNode[TopologicalOrder[i]].V += LinkAlpha[nowLink] * LinkV[nowLink];
                    SubNode[TopologicalOrder[i]].S += LinkAlpha[nowLink] * LinkAlpha[nowLink] * LinkS[nowLink];
                }
            }
        }

        #endregion
    }
}
