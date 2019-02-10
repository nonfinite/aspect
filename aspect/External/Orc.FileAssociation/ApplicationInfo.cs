// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApplicationInfo.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.FileAssociation
{
    using System.Collections.Generic;

    public class ApplicationInfo
    {
        #region Constructors
        public ApplicationInfo(string company, string name, string title, string location)
        {
            Company = company;
            Name = name;
            Title = title;
            Location = location;
            SupportedExtensions = new List<string>();
        }
        
        #endregion

        #region Properties
        public string Company { get; private set; }

        public string Name { get; private set; }

        public string Title { get; private set; }

        public string Location { get; private set; }

        public List<string> SupportedExtensions { get; private set; }
        #endregion
    }
}
