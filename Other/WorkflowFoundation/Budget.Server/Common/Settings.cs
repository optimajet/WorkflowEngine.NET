using System.Configuration;
using Budget2.DAL;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using Common.Utils;

namespace Common
{
	public class Settings: Budget2DataContextService
	{
        Guid? currentBudgetVersionId = null;
        public Guid? CurrentBudgetVersionId
        {
            get
            {
                if (!currentBudgetVersionId.HasValue)
                {
                    using (var context = CreateContext())
                    {
                        int? nBudgetYear = BudgetYearForUpload;
                        int budgetYear = nBudgetYear.HasValue ? nBudgetYear.Value : DateTime.Now.Year;
                        var bvList = (from bv in context.BudgetVersions where bv.IsCurrent == true && bv.Budget.Name == budgetYear select bv).FirstOrDefault();
                        if (bvList != null)
                            currentBudgetVersionId = bvList.Id;
                    }
                }

                return currentBudgetVersionId;

            }
        }

       #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Public event used to indicate in settings have been changed.
        /// </summary>
        public static event EventHandler<EventArgs> Changed;
        /// <summary>
        /// Private member to hold singleton instance.
        /// </summary>
        private static Settings SettingsSingleton;
        /// <summary>
        /// Private member to hold the title of the blog.
        /// </summary>
        private string siteName = String.Empty;
        /// <summary>
        /// Private member to hold a brief synopsis of the blog.
        /// </summary>
        private string siteDescription = String.Empty;
        /// <summary>
        /// Private member to hold blog storage location.
        /// </summary>
        private string storageLocation = String.Empty;
        /// <summary>
        /// Private member to hold the email address notifications are sent to.
        /// </summary>
        private string emailAddress = String.Empty;
        /// <summary>
        /// Private member to hold the SMTP server to contact when sending email.
        /// </summary>
        private string smtpServer = String.Empty;
        /// <summary>
        /// Private member to hold the SMTP port number.
        /// </summary>
        private int smtpServerPort = Int32.MinValue;
        /// <summary>
        /// Private member to hold the username used when contacting the SMTP server.
        /// </summary>  
        private string smtpUsername = String.Empty;
        /// <summary>
        /// Private member to hold the password used when contacting the SMTP server.
        /// </summary>
        private string smtpPassword = String.Empty;
        private string culture;
        private double timezone;
        #endregion

        #region Settings()
        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        private Settings()
        {
            try
            {
                Load();
            }
            catch (Exception ex)
            {
                Logger.Log.Fatal(null, ex);
            }
        }
        #endregion

        #region Instance

        static volatile object _sync = new object();
        /// <summary>
        /// Gets the singleton instance of the <see cref="Settings"/> class.
        /// </summary>
        /// <value>A singleton instance of the <see cref="Settings"/> class.</value>
        /// <remarks></remarks>
        public static Settings Instance
        {
            get
            {
                if (SettingsSingleton == null)
                {
                    lock (_sync)
                    {
                        if (SettingsSingleton == null)
                            SettingsSingleton = new Settings();
                    }
                }
            
                return SettingsSingleton;
            }
        }
        #endregion

        #region	GENERAL SETTINGS
        #region Description
        /// <summary>
        /// Gets or sets the description of the blog.
        /// </summary>
        /// <value>A brief synopsis of the blog content.</value>
        /// <remarks>This value is also used for the description meta tag.</remarks>
        public string Description
        {
            get
            {
                return siteDescription;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    siteDescription = String.Empty;
                }
                else
                {
                    siteDescription = value;
                }
            }
        }
        #endregion

        #region Name
        /// <summary>
        /// Gets or sets the name of the blog.
        /// </summary>
        /// <value>The title of the blog.</value>
        public string Name
        {
            get
            {
                return siteName;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    siteName = String.Empty;
                }
                else
                {
                    siteName = value;
                }
            }
        }
        #endregion

        #region StorageLocation
        /// <summary>
        /// Gets or sets the default storage location for blog data.
        /// </summary>
        /// <value>The default storage location for blog data.</value>
        public string StorageLocation
        {
            get
            {
                return storageLocation;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    storageLocation = String.Empty;
                }
                else
                {
                    storageLocation = value;
                }
            }
        }
        #endregion

        #region MobileTheme

        private string _MobileTheme;
        /// <summary>
        /// Gets or sets the mobile theme.
        /// </summary>
        /// <value>The mobile theme.</value>
        public string MobileTheme
        {
            get { return _MobileTheme; }
            set { _MobileTheme = value; }
        }

        #endregion


        //============================================================
        //	DATABASE SETTINGS
        //============================================================
        #region MSSQLConnectionString
        /// <summary>
        /// Gets or sets the connection string used to connect to the SQL database.
        /// </summary>
        public string MSSQLConnectionString
        {
            get;
            set;
        }
        #endregion

        //============================================================
        //	EMAIL SETTINGS
        //============================================================
        #region Email
        /// <summary>
        /// Gets or sets the e-mail address notifications are sent to.
        /// </summary>
        /// <value>The e-mail address notifications are sent to.</value>
        public string Email
        {
            get
            {
                return emailAddress;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    emailAddress = String.Empty;
                }
                else
                {
                    emailAddress = value;
                }
            }
        }
        #endregion

        #region SmtpPassword
        /// <summary>
        /// Gets or sets the password used to connect to the SMTP server.
        /// </summary>
        /// <value>The password used to connect to the SMTP server.</value>
        public string SmtpPassword
        {
            get
            {
                return smtpPassword;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    smtpPassword = String.Empty;
                }
                else
                {
                    smtpPassword = value;
                }
            }
        }
        #endregion

        #region SmtpServer
        /// <summary>
        /// Gets or sets the DNS name or IP address of the SMTP server used to send notification emails.
        /// </summary>
        /// <value>The DNS name or IP address of the SMTP server used to send notification emails.</value>
        public string SmtpServer
        {
            get
            {
                return smtpServer;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    smtpServer = String.Empty;
                }
                else
                {
                    smtpServer = value;
                }
            }
        }
        #endregion

        #region SmtpServerPort
        /// <summary>
        /// Gets or sets the DNS name or IP address of the SMTP server used to send notification emails.
        /// </summary>
        /// <value>The DNS name or IP address of the SMTP server used to send notification emails.</value>
        public int SmtpServerPort
        {
            get
            {
                return smtpServerPort;
            }

            set
            {
                smtpServerPort = value;
            }
        }
        #endregion

        #region SmtpUsername
        /// <summary>
        /// Gets or sets the user name used to connect to the SMTP server.
        /// </summary>
        /// <value>The user name used to connect to the SMTP server.</value>
        public string SmtpUserName
        {
            get
            {
                return smtpUsername;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    smtpUsername = String.Empty;
                }
                else
                {
                    smtpUsername = value;
                }
            }
        }
        #endregion

        #region EmailSubjectPrefix
        private string _EmailSubjectPrefix;
        /// <summary>
        /// Gets or sets the email subject prefix.
        /// </summary>
        /// <value>The email subject prefix.</value>
        public string EmailSubjectPrefix
        {
            get { return _EmailSubjectPrefix; }
            set { _EmailSubjectPrefix = value; }
        }
        #endregion
        
        #region Culture
        /// <summary>
        /// Gets or sets the name of the author of this blog.
        /// </summary>
        /// <value>The name of the author of this blog.</value>
        public string Culture
        {
            get { return culture; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    culture = String.Empty;
                }
                else
                {
                    culture = value;
                }
            }
        }
        #endregion

        #region Timezone
        /// <summary>
        /// Gets or sets the maximum number of characters that are displayed from a blog-roll retrieved post.
        /// </summary>
        /// <value>The maximum number of characters to display.</value>
        public double Timezone
        {
            get { return timezone; }
            set { timezone = value; }
        }
        #endregion

        #endregion

        #region Load()
        /// <summary>
        /// Initializes the singleton instance of the <see cref="Settings"/> class.
        /// </summary>
        private void Load()
        {
            Type settingsType = this.GetType();

            //------------------------------------------------------------
            //	Enumerate through individual settings nodes
            //------------------------------------------------------------
            List<Budget2.DAL.AppSetting> dic = null;

            using (var context = this.CreateContext())
            {
                dic = context.AppSettings.ToList();
            }

            foreach (var key in dic)
            {
                //------------------------------------------------------------
                //	Extract the setting's name/value pair
                //------------------------------------------------------------
                string name = key.Name;
                string value = key.Value;

                //------------------------------------------------------------
                //	Enumerate through public properties of this instance
                //------------------------------------------------------------
                foreach (PropertyInfo propertyInformation in settingsType.GetProperties())
                {
                    //------------------------------------------------------------
                    //	Determine if configured setting matches current setting based on name
                    //------------------------------------------------------------
                    if (propertyInformation.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        //------------------------------------------------------------
                        //	Attempt to apply configured setting
                        //------------------------------------------------------------
                        try
                        {
                            propertyInformation.SetValue(this, Convert.ChangeType(value, propertyInformation.PropertyType, CultureInfo.CurrentCulture), null);
                        }
                        catch (Exception ex)
                        {
                            Logger.Log.Error(null, ex);
                        }
                        break;
                    }
                }
            }
            // storageLocation = Providers.BlogService.GetStorageLocation();
        }
        #endregion

        #region OnChanged()
        /// <summary>
        /// Occurs when the settings have been changed.
        /// </summary>
        private static void OnChanged()
        {
            //------------------------------------------------------------
            //	Attempt to raise the Changed event
            //------------------------------------------------------------
            try
            {
                //------------------------------------------------------------
                //	Execute event handler
                //------------------------------------------------------------
                if (Settings.Changed != null)
                {
                    Settings.Changed(null, new EventArgs());
                }
            }
            catch
            {
                //------------------------------------------------------------
                //	Rethrow exception
                //------------------------------------------------------------
                throw;
            }
        }
        #endregion

        #region Save()
        /// <summary>
        /// Saves the settings to disk.
        /// </summary>
        public void Save()
        {
            using (var context = this.CreateContext())
            {
                Type settingsType = this.GetType();
                var dic = (from request in context.AppSettings select request).ToList();
                //------------------------------------------------------------
                //	Enumerate through settings properties
                //------------------------------------------------------------
                foreach (PropertyInfo propertyInformation in settingsType.GetProperties())
                {
                    try
                    {
                        if (propertyInformation.Name != "Instance")
                        {
                            //------------------------------------------------------------
                            //	Extract property value and its string representation
                            //------------------------------------------------------------
                            object propertyValue = propertyInformation.GetValue(this, null);
                            string valueAsString = propertyValue.ToString();

                            //------------------------------------------------------------
                            //	Format null/default property values as empty strings
                            //------------------------------------------------------------
                            if (propertyValue.Equals(null))
                            {
                                valueAsString = String.Empty;
                            }
                            if (propertyValue.Equals(Int32.MinValue))
                            {
                                valueAsString = String.Empty;
                            }
                            if (propertyValue.Equals(Single.MinValue))
                            {
                                valueAsString = String.Empty;
                            }

                            //------------------------------------------------------------
                            //	Write property name/value pair
                            //------------------------------------------------------------
                            var item = dic.Where(c => c.Name == propertyInformation.Name).FirstOrDefault();
                            if (item == null)
                            {
                                item = new AppSetting() { Name = propertyInformation.Name, Value = valueAsString };
                                context.AppSettings.InsertOnSubmit(item);
                                dic.Add(item);
                            }
                            else
                            {
                                item.Value = valueAsString;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log.Error(null, ex);
                    }
                }

                context.SubmitChanges();
            }

            OnChanged();
        }
        #endregion

        #region Version()

        private static string _Version;
        /// <summary>
        /// Returns the BlogEngine.NET version information.
        /// </summary>
        /// <value>The BlogEngine.NET major, minor, revision, and build numbers.</value>
        /// <remarks>The current version is determined by extracting the build version of the BlogEngine.Core assembly.</remarks>
        public string Version()
        {
            if (_Version == null)
                _Version = GetType().Assembly.GetName().Version.ToString();

            return _Version;
        }
        #endregion

        #region Bussiness

        private decimal _billDemandWFLimitSum = 500000;
        public decimal BillDemandWfLimitSum
        {
            get
            {
                return _billDemandWFLimitSum;
            }

            set
            {
                _billDemandWFLimitSum = value;
            }
        }

        #endregion

	    private int? BudgetYearForUpload
	    {
	        get
	        {
                return StringToNullableConverter.GetPositiveInt(ConfigurationManager.AppSettings["BudgetYearForUpload"]);
	        }
	    }
    }
}