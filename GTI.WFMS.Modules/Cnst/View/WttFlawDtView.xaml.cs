﻿using GTI.WFMS.Modules.Cnst.Model;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace GTI.WFMS.Modules.Cnst.View
{
    /// <summary>
    /// WttFlawDtView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WttFlawDtView : UserControl
    {

        private string CNT_NUM;

        public WttFlawDtView(string _CNT_NUM)
        {
            InitializeComponent();

            this.CNT_NUM = _CNT_NUM;
            this.txtCNT_NUM.EditValue = _CNT_NUM; //키를 뷰모델로 넘기기위해 뷰바인딩 활용
        }




        /// <summary>
        /// 헤더 All 체크
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AllChk_Checked(object sender, RoutedEventArgs e)
        {
            //CheckEdit ce = sender as CheckEdit;
            //bool chk = ce.IsChecked is bool;
            foreach (WttFlawDt row in ((List<WttFlawDt>)grid.ItemsSource))
            {
                row.CHK = "True";
            }
        }
        private void AllChk_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (WttFlawDt row in ((List<WttFlawDt>)grid.ItemsSource))
            {
                row.CHK = null;
            }
        }


    }
}
