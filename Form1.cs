using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace ControlPoint_AuditLogFirstDateExtractor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public void ChangeDaysQtyTextInAnotherThread(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(ChangeDaysQtyTextInAnotherThread), new object[] { value });
                return;
            }
            lblDaysQty.Text = value;
        }

        public void ChangeToolStripTextInAnotherThread(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(ChangeToolStripTextInAnotherThread), new object[] { value });
                return;
            }
            toolStripStatusLabel1.Text = value;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            //new Thread(startIndexing).Start();
            startIndexing();
        }

        private void startIndexing()
        {

            ChangeToolStripTextInAnotherThread("Indexing...");

            DateTime minDate = DateTime.Now;
            System.TimeSpan diffResult;

            SPSecurity.RunWithElevatedPrivileges(delegate
            {

                for (int i = 1; i <= lstSiteCollections.SelectedItems.Count; i++)
                {

                    using (SPSite siteCollection = new SPSite(lstSiteCollections.SelectedItems[i - 1].ToString()))
                    {
                        SPAuditQuery wssQuery = new SPAuditQuery(siteCollection);
                        wssQuery.RowLimit = 10000;
                        SPAuditEntryCollection auditCol = siteCollection.Audit.GetEntries(wssQuery);
                        
                        foreach (SPAuditEntry entry in auditCol)
                        {
                            if (entry.Occurred < minDate)
                            {
                                minDate = entry.Occurred;
                                diffResult = DateTime.Now.Subtract(minDate);
                                ChangeDaysQtyTextInAnotherThread(diffResult.Days.ToString());
                            }
                        }
                        
                        auditCol = null;
                        wssQuery = null;
                    }

                }

            });

            //Calculate date difference between now and minDate and assign to label.

            diffResult = DateTime.Now.Subtract(minDate);
            lblDaysQty.Text = diffResult.Days.ToString();
            lblDaysQty.ForeColor = Color.Red;

            ChangeToolStripTextInAnotherThread("Ready");

        }


        private void Form1_Load(object sender, EventArgs e)
        {

            //load sitecollections unto list
            lstSiteCollections.Items.Clear();

            SPSecurity.RunWithElevatedPrivileges(delegate
            {

                SPServiceCollection services = SPFarm.Local.Services;
                foreach (SPService curService in services)
                {
                    if (curService is SPWebService)
                    {
                        SPWebService webService = (SPWebService)curService;

                        foreach (SPWebApplication webApp in webService.WebApplications)
                        {
                            foreach (SPSite sc in webApp.Sites)
                            {
                                try
                                {
                                    lstSiteCollections.Items.Add(sc.Url);
                                }
                                catch (Exception ex)
                                {
                                    //Console.WriteLine("Exception occured: {0}\r\n{1}", ex.Message, ex.StackTrace);
                                }
                            }
                        }
                    }
                }

            });

            ChangeToolStripTextInAnotherThread("Ready");
        }
    }
}
