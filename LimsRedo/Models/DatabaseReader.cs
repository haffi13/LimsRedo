﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace LimsRedo
{
    public class DatabaseReader
    {
        private static string connectionString =
        "Server=EALSQL1.eal.local; Database= DB2017_C08; User Id=USER_C08; Password=SesamLukOp_08";

        private List<string> SampleType = new List<string>();

        public List<string> GetSampleByValue(string searchValue, string spParameter)
        {
            List<string> ret = new List<string>();
            string storedProcedure = GetStoredProcedureByParameter(spParameter);
            List<int> sampleID = GetSampleTypeAndID(searchValue, spParameter, storedProcedure);
            for (int i = 0; i < sampleID.Count; i++)
            {
                ret.Add(GetSampleWithSampleTypeAndID(SampleType[i], sampleID[i]));
            }
            return ret;
        }
        public List<string> GetSampleByID(int sampleID)
        {
            string sampleType = string.Empty;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    SqlCommand GetSampleType = new SqlCommand("spGetSampleTypeByID", con);
                    GetSampleType.CommandType = CommandType.StoredProcedure;
                    GetSampleType.Parameters.Add(new SqlParameter("@Sample_ID", sampleID));
                    SqlDataReader reader = GetSampleType.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            sampleType = reader["Sample_Type"].ToString();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Sample not found!");/////////-----------------------delegates instead ?
                    }
                }
                catch (SqlException e)
                {
                    MessageBox.Show(e.Message);////////////---------------------------------
                }
            }
            List<string> ret = new List<string>();
            ret.Add(GetSampleWithSampleTypeAndID(sampleType, sampleID));
            return ret;
        }
        private string GetStoredProcedureByParameter(string spParameter)
        {
            string storedProcedure = string.Empty;
            switch (spParameter)
            {
                case "@Antibody":
                    storedProcedure = "spGetSampleByAntibody";
                    break;
                case "@Cell_Type":
                    storedProcedure = "spGetSampleByCellType";
                    break;
                case "@Condition":
                    storedProcedure = "spGetSampleByCondition";
                    break;
                case "@Initials":
                    storedProcedure = "spGetSampleByInitials";
                    break;
                case "@PI_Value":
                    storedProcedure = "spGetSampleByPI";
                    break;
                case "@Treatment":
                    storedProcedure = "spGetSampleByTreatment";
                    break;
            }
            return storedProcedure;
        }
        private List<int> GetSampleTypeAndID(string searchValue, string spParameter, string storedProcedure)
        {
            List<int> sampleID = new List<int>();
            SampleType.Clear();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    SqlCommand command = new SqlCommand(storedProcedure, con);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter(spParameter, searchValue));
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        //Runs through this loop for any sample id relevant to the value the user looked for.
                        while (reader.Read())
                        {
                            if (!sampleID.Contains(int.Parse(reader["Sample_ID"].ToString())))
                            {
                                sampleID.Add(int.Parse(reader["Sample_ID"].ToString()));
                                SampleType.Add(reader["Sample_Type"].ToString());
                            }
                        }
                    }
                }
                catch (SqlException e)
                {
                    MessageBox.Show(e.Message.ToString());//    fixit!!!!!!!!!!!!!!!!!!!!!!!
                }
            }
            return sampleID;
        }
        private string GetSampleWithSampleTypeAndID(string sampleType, int sampleID)
        {
            string nl = "\n";
            string ret = string.Empty;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("spGetSampleInfoFromIDAndType", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Sample_ID", SqlDbType.Int).Value = sampleID;
                    cmd.Parameters.Add("@Sample_Type", SqlDbType.VarChar).Value = sampleType;
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            #region reader to string                           
                            string GenomeType = reader["Genome_Type"].ToString();
                            string CellType = reader["Cell_Type"].ToString();
                            string Treatment = reader["Treatment"].ToString();
                            string Condition = reader["Condition"].ToString();
                            string Comments = reader["Comments"].ToString();
                            string Concentration = reader["Concentration"].ToString();
                            string Volume = reader["Volume"].ToString();
                            string Initials = reader["Initials"].ToString();
                            string PiValue = reader["Pi_Value"].ToString();
                            string DateOfAddition = reader["Date_Of_Addition"].ToString();
                            #endregion
                            ret = "Sample ID:          " + sampleID.ToString() + nl +
                                  "Sample Type:        " + sampleType + nl +
                                  "Cell Type:          " + CellType + nl +
                                  "Treatment:          " + Treatment + nl +
                                  "Condition:          " + Condition + nl +
                                  "Comments:           " + Comments + nl +
                                  "Concentration:      " + Concentration + nl +
                                  "Volume:             " + Volume + nl +
                                  "Initials:           " + Initials + nl +
                                  "PI Value:           " + PiValue + nl +
                                  "Date:               " + DateOfAddition + nl;
                            #region sample specific data
                            switch (sampleType)
                            {
                                case "ATAC-Seq":
                                    string TransposaseUnit = reader["Transposase_Unit"].ToString();
                                    string PCRCycles = reader["PCR_Cycles"].ToString();
                                    ret += "Transposase Unit:   " + TransposaseUnit + nl +
                                           "PCR Cycles:         " + PCRCycles;
                                    break;
                                case "Hi-C":
                                    string RestrictionEnzyme = reader["Restriction_Enzyme"].ToString();
                                    PCRCycles = reader["PCR_Cycles"].ToString();
                                    ret += "Restriction Enzyme: " + RestrictionEnzyme + nl +
                                           "PCR Cycles:         " + PCRCycles + nl;
                                    break;
                                case "RNA-Seq":
                                    string PrepType = reader["Prep_Type"].ToString();
                                    string RIN = reader["RIN"].ToString();
                                    ret += "Prep Type:          " + PrepType + nl +
                                           "RIN:                " + RIN + nl;
                                    break;
                                case "ChIP-Seq":
                                    string Antibody = reader["Antibody"].ToString();
                                    string AntibodyLot = reader["Antibody_Lot"].ToString();
                                    string AntibodyCatalogueNumber = reader["Antibody_Catalogue_Number"].ToString();
                                    ret += "Atibody:            " + Antibody + nl +
                                           "Antibody Lot:       " + AntibodyLot + nl +
                                           "Antibody Cat. Nr:   " + AntibodyCatalogueNumber + nl;
                                    break;
                                    #endregion
                            }
                        }
                    }
                }
                catch (SqlException e)
                {
                    MessageBox.Show(e.Message);///////7----------------------------------------
                }
            }
            return ret;
        }
    }
}
