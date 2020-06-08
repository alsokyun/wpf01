﻿using DevExpress.Xpf.Editors;
using GTI.WFMS.GIS.Module.View;
using GTI.WFMS.Models.Common;
using GTI.WFMS.Models.Fclt.Model;
using GTI.WFMS.Models.Pipe.Model;
using GTIFramework.Common.Log;
using GTIFramework.Common.MessageBox;
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace GTI.WFMS.GIS.Module.ViewModel
{
    public class UC_VALV_PSModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propNm)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propNm));
            }
        }



        #region ==========  Properties 정의 ==========
        /// <summary>
        /// Loaded Event
        /// </summary>
        public RelayCommand<object> LoadedCommand { get; set; }
        public RelayCommand<object> SaveCommand { get; set; }
        public RelayCommand<object> DelCommand { get; set; }


        /// <summary>
        /// 시설물 키 : FctDtl과 별도로 관리됨
        /// </summary>
        private string __FTR_CDE;
        public string FTR_CDE
        {
            get { return __FTR_CDE; }
            set
            {
                this.__FTR_CDE = value;
                OnPropertyChanged("FTR_CDE");
            }
        }
        private string __FTR_IDN;
        public string FTR_IDN
        {
            get { return __FTR_IDN; }
            set
            {
                this.__FTR_IDN = value;
                OnPropertyChanged("FTR_IDN");
            }
        }


        private ValvFacDtl fctDtl = new ValvFacDtl();
        public ValvFacDtl FctDtl
        {
            get{ return fctDtl;}
            set
            {
                fctDtl = value;
                OnPropertyChanged("FctDtl");
            }
        }

        #endregion


        #region ==========  Member 정의 ==========
        UC_VALV_PS uC_VALV_PS;
        Button btnSave;


        #endregion




        /// 생성자
        public UC_VALV_PSModel()
        {
            this.LoadedCommand = new RelayCommand<object>(OnLoaded);
            this.SaveCommand = new RelayCommand<object>(OnSave);    
            this.DelCommand = new RelayCommand<object>(OnDelete);
        }

        #region ==========  이벤트 핸들러 ==========

        /// <summary>
        /// 로딩작업
        /// </summary>
        /// <param name="obj"></param>
        private void OnLoaded(object obj)
        {
            try
            {
                // 0.화면객체인스턴스화
                if (obj == null) return;

                uC_VALV_PS = obj as UC_VALV_PS;

                btnSave = uC_VALV_PS.btnSave;
                
                //2.화면데이터객체 초기화
                InitDataBinding();


                //3.권한처리
                //permissionApply();

                // 4.초기조회
                InitModel();
                               
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }


        

        /// <summary>
        /// 저장작업
        /// </summary>
        /// <param name="obj"></param>
        private void OnSave(object obj)
        {
            // 필수체크 (Tag에 필수체크 표시한 EditBox, ComboBox 대상으로 수행)
            if (!BizUtil.ValidReq(uC_VALV_PS)) return;


            if (Messages.ShowYesNoMsgBox("저장하시겠습니까?") != MessageBoxResult.Yes) return;

            try
            {
                FctDtl.FTR_CDE = this.FTR_CDE;
                FctDtl.FTR_IDN = Convert.ToInt32(this.FTR_IDN); //신규위치 및 기존위치 정보만 있을수 있으므로 shape의 관리번호를 기준으로한다.
                BizUtil.Update2(FctDtl, "SaveValvFacDtl");

                //2.위치정보 - 위치편집한 경우만
                if (!FmsUtil.IsNull(GisCmm.WKT_POINT))
                {
                    GisCmm.SavePoint(FctDtl.FTR_CDE, FctDtl.FTR_IDN.ToString(), "WTL_VALV_PS");
                    GisCmm.WKT_POINT = "";
                }

            }
            catch (Exception e)
            {
                Messages.ShowErrMsgBox("저장 처리중 오류가 발생하였습니다." + e.Message);
                return;
            }

            Messages.ShowOkMsgBox();
            InitModel();
        }


        /// <summary>
        /// 삭제처리
        /// </summary>
        /// <param name="obj"></param>
        private void OnDelete(object obj)
        {
            //0.삭제전 체크
            Hashtable param = new Hashtable();
            param.Add("sqlId", "selectChscResSubList");
            param.Add("sqlId2", "SelectFileMapList");

            param.Add("FTR_CDE", this.FTR_CDE);
            param.Add("FTR_IDN", this.FTR_IDN);
            param.Add("BIZ_ID", string.Concat(this.FTR_CDE, this.FTR_IDN));

            Hashtable result = BizUtil.SelectLists(param);
            DataTable dt = new DataTable();
            DataTable dt2 = new DataTable();

            try
            {
                dt = result["dt"] as DataTable;
                if (dt.Rows.Count > 0)
                {
                    Messages.ShowInfoMsgBox("유지보수내역이 존재합니다.");
                    return;
                }
            }
            catch (Exception) { }
            try
            {
                dt2 = result["dt2"] as DataTable;
                if (dt2.Rows.Count > 0)
                {
                    Messages.ShowInfoMsgBox("파일첨부내역이 존재합니다.");
                    return;
                }
            }
            catch (Exception) { }

            // 1.삭제처리
            if (Messages.ShowYesNoMsgBox("변류시설를 삭제하시겠습니까?") != MessageBoxResult.Yes) return;
            try
            {
                BizUtil.Update2(this.FctDtl, "deleteValvFacDtl");
            }
            catch (Exception)
            {
                Messages.ShowErrMsgBox("삭제 처리중 오류가 발생하였습니다.");
                return;
            }
            Messages.ShowOkMsgBox();

            InitModel();

        }
        #endregion

        #region ============= 메소드정의 ================

        // 초기조회
        private void InitModel()
        {
            Hashtable param = new Hashtable();
            param.Add("sqlId", "SelectValvFacDtl");
            param.Add("FTR_CDE", this.FTR_CDE);
            param.Add("FTR_IDN", this.FTR_IDN);

            ValvFacDtl result = BizUtil.SelectObject(param) as ValvFacDtl;
            if (result != null)
            {
                this.FctDtl = result;
            }
            else
            {
                //신규등록이면 상세화면표시
                if (!"Y".Equals(uC_VALV_PS.btnDel.Tag))
                {
                    uC_VALV_PS.grid.Visibility = Visibility.Hidden; //DB데이터가 없으면 빈페이지표시
                }
            }
        }


        /// <summary>
        /// 초기화 바인딩
        /// </summary>
        private void InitDataBinding()
        {
            try
            {
                // cbHJD_CDE 행정동
                BizUtil.SetCombo(uC_VALV_PS.cbHJD_CDE, "Select_ADAR_LIST", "HJD_CDE", "HJD_NAM", "선택");

                // cbMNG_CDE 관리기관
                BizUtil.SetCmbCode(uC_VALV_PS.cbMNG_CDE, "250101", "선택");
                // cbVAL_MOF 형식
                BizUtil.SetCmbCode(uC_VALV_PS.cbVAL_MOF, "250016", "선택");

                // cbVAL_MOP 관재질
                BizUtil.SetCmbCode(uC_VALV_PS.cbVAL_MOP, "250015", "선택");

                // cbSAE_CDE    제수변회전방향
                BizUtil.SetCmbCode(uC_VALV_PS.cbSAE_CDE, "250027", "선택");

                // cbMTH_CDE    제수변구동방법
                BizUtil.SetCmbCode(uC_VALV_PS.cbMTH_CDE, "250065", "선택");

                // cbVAL_FOR    시설물형태(=구조물형태)
                BizUtil.SetCmbCode(uC_VALV_PS.cbVAL_FOR, "250007", "선택");

                // cbCST_CDE    이상상태
                BizUtil.SetCmbCode(uC_VALV_PS.cbCST_CDE, "250062", "선택");

                // cbOFF_CDE    개폐여부
                BizUtil.SetCmbCode(uC_VALV_PS.cbOFF_CDE, "250036", "선택");
            }
            catch (Exception ex)
            {
                Messages.ShowErrMsgBoxLog(ex);
            }
        }


        /// <summary>
        /// 화면 권한처리
        /// </summary>
        private void permissionApply()
        {
            try
            {
                string strPermission = Logs.htPermission[Logs.strFocusMNU_CD].ToString();
                switch (strPermission)
                {
                    case "W":
                        break;
                    case "R":
                        btnSave.Visibility = Visibility.Collapsed;
                        break;
                    case "N":
                        break;
                }

            }
            catch (Exception ex)
            {
                Messages.ShowErrMsgBoxLog(ex);
            }

        }

        #endregion


    }
}
