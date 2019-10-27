using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MYKCalculator
{
    class Spreadsheet
    {
        private string sv_cluster;
        private string array_id;
        private string lab_id;
        private string entitaet;
        private string score;
        private string numberOf;
        private string single;
        private string reziprok;
        private string komplex;
        private string from_pos;
        private string from_direct;
        private string to_chrom;
        private string chr_band;
        private string to_pos;
        private string to_direct;
        private string partnergen;
        private string tumor_ref_pr;
        private string tumor_alt_pr;
        private string vaf_PR;
        private string tumor_ref_sr;
        private string tumor_alt_sr;
        private string vaf_sr;
        private string vaf_mean;
        private string vaf_differez;
        private string reads_mean;
        private string reads_differenz;
        private string myc;
        private string n_lym;
        private string n_myl;
        private string mll_freq;
        private string fish;
        private string signalkonstellation_myc;
        private string fish_8_14;
        private string signalkonstellation_8_14;
        private string klongroeße;

        public string Sv_cluster { get => sv_cluster; set => sv_cluster = value; }
        public string Array_id { get => array_id; set => array_id = value; }
        public string Lab_id { get => lab_id; set => lab_id = value; }
        public string Entitaet { get => entitaet; set => entitaet = value; }
        public string Score { get => score; set => score = value; }
        public string NumberOf { get => numberOf; set => numberOf = value; }
        public string Single { get => single; set => single = value; }
        public string Reziprok { get => reziprok; set => reziprok = value; }
        public string Komplex { get => komplex; set => komplex = value; }
        public string From_pos { get => from_pos; set => from_pos = value; }
        public string From_direct { get => from_direct; set => from_direct = value; }
        public string To_chrom { get => to_chrom; set => to_chrom = value; }
        public string Chr_band { get => chr_band; set => chr_band = value; }
        public string To_pos { get => to_pos; set => to_pos = value; }
        public string To_direct { get => to_direct; set => to_direct = value; }
        public string Partnergen { get => partnergen; set => partnergen = value; }
        public string Tumor_ref_pr { get => tumor_ref_pr; set => tumor_ref_pr = value; }
        public string Tumor_alt_pr { get => tumor_alt_pr; set => tumor_alt_pr = value; }
        public string Vaf_PR { get => vaf_PR; set => vaf_PR = value; }
        public string Tumor_ref_sr { get => tumor_ref_sr; set => tumor_ref_sr = value; }
        public string Tumor_alt_sr { get => tumor_alt_sr; set => tumor_alt_sr = value; }
        public string Vaf_sr { get => vaf_sr; set => vaf_sr = value; }
        public string Vaf_mean { get => vaf_mean; set => vaf_mean = value; }
        public string Vaf_differez { get => vaf_differez; set => vaf_differez = value; }
        public string Reads_mean { get => reads_mean; set => reads_mean = value; }
        public string Reads_differenz { get => reads_differenz; set => reads_differenz = value; }
        public string Myc { get => myc; set => myc = value; }
        public string N_lym { get => n_lym; set => n_lym = value; }
        public string N_myl { get => n_myl; set => n_myl = value; }
        public string Mll_freq { get => mll_freq; set => mll_freq = value; }
        public string Fish { get => fish; set => fish = value; }
        public string Signalkonstellation_myc { get => signalkonstellation_myc; set => signalkonstellation_myc = value; }
        public string Fish_8_14 { get => fish_8_14; set => fish_8_14 = value; }
        public string Signalkonstellation_8_14 { get => signalkonstellation_8_14; set => signalkonstellation_8_14 = value; }
        public string Klongroeße { get => klongroeße; set => klongroeße = value; }
    }
}
