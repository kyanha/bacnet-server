using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using BACnetLibrary;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace BACnetLibrary
{
    public class myTreeNode : TreeNode
    {
        public enum TREENODE_OBJ_TYPE
        {
            NetworkNumber = 33,
            LastAccessTime,
            BACnetObject,
            Device,
            BACnetPort,
            GenericText,
            State,
            SiteIPEP
        }

        public TREENODE_OBJ_TYPE type;

        public Device device = new Device();

        public IPEndPoint ipep;

        public long lastHeardFromTime;
        public uint networkNumber;
        public BACnetObjectIdentifier oID;

        // We can determine our local (directly connected) network number from the single network number
        // in a router table that is not reported by the who-is-router request. These flags help do that.
        public bool networkNumberFromWhoIsRouter;
        public bool networkNumberFromInitRouterTable;

        public myTreeNode()
        {
        }

        public myTreeNode(string text)
        {
            Text = text;
        }

        public myTreeNode(TREENODE_OBJ_TYPE tnoType, string text)
        {
            Text = TextFromType(tnoType) + " " + text;
            this.type = tnoType;
        }

        public myTreeNode(TREENODE_OBJ_TYPE tnoType, string text, System.Drawing.Color clr )
        {
            Text = TextFromType(tnoType) + " " + text;
            this.type = tnoType;
            this.BackColor = clr;
        }

        public myTreeNode(TREENODE_OBJ_TYPE tnoType)
        {
            this.type = tnoType;
            Text = TextFromType(tnoType);
        }

        string TextFromType(TREENODE_OBJ_TYPE tnoType)
        {
            switch (tnoType)
            {
                case TREENODE_OBJ_TYPE.NetworkNumber:
                    return "Network Number ";
                case TREENODE_OBJ_TYPE.SiteIPEP:
                    return "Site IP Addr   ";
                case TREENODE_OBJ_TYPE.State:
                    return "State          ";
                case TREENODE_OBJ_TYPE.BACnetPort:
                    return "Port           ";
                case TREENODE_OBJ_TYPE.LastAccessTime:
                    return "Last comms     ";
            }
            return tnoType.ToString();
        }

        public myTreeNode AddMyTreeNodeObject(TREENODE_OBJ_TYPE tnoType, string text)
        {
            myTreeNode mtn = new myTreeNode(text);
            mtn.type = tnoType;
            base.Nodes.Add(mtn);
            return mtn;
        }

        public void UpdatemyTreeNodeLeaf(myTreeNode.TREENODE_OBJ_TYPE objType, string text)
        {
            // search for a subnode that matches objType and update the text field
            if (Nodes.Count > 0) foreach (myTreeNode mtn in Nodes)
                {
                    if (mtn.type == objType)
                    {
                        mtn.Text = TextFromType(objType) + " " + text;
                    }
                }
        }

        public void UpdatemyTreeNodeLeaf(myTreeNode.TREENODE_OBJ_TYPE objType, string text, System.Drawing.Color clr )
        {
            // search for a subnode that matches objType and update the text field
            if (Nodes.Count > 0) foreach (myTreeNode mtn in Nodes)
                {
                    if (mtn.type == objType)
                    {
                        mtn.Text = TextFromType(objType) + " " + text;
                        mtn.BackColor = clr;
                    }
                }
        }
    }
}
