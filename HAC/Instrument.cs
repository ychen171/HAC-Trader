﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraderAPI;
using TradeMatching;

namespace HAC
{
    class Instrument
    {
        private InstrNotifyClass m_Notify;
        private InstrObjClass m_Instr;
        private OrderSetClass m_OrderSet;
        private TradeMatcher m_Matcher;
        private double m_TickSize;

        public event OnInstrumentUpdateEventHandler OnInstrumentUpdate;
        public event OnInstrumentFillEventHandler OnInstrumentFill;

        public Instrument()
        {
            m_Matcher = new TradeMatcher(RoundTurnMethod.FIFO);


            // Create a new InstrObjClass object
            m_Instr = new InstrObjClass();

            // Create a new InstrNotifyClass object from the InstrObjectClass object.
            m_Notify = (InstrNotifyClass)m_Instr.CreateNotifyObj;
            // Enable price updates.
            m_Notify.EnablePriceUpdates = true;
            // Set UpdateFilter so event will fire anytime any one of these changes in the 
            // associated InstrObjClass object.
            m_Notify.UpdateFilter = "LAST, LASTQTY";
            // Subscribe to the OnNotifyUpdate event.
            m_Notify.OnNotifyUpdate += new InstrNotifyClass.OnNotifyUpdateEventHandler(OnNotifyUpdate);
            // Set the exchange, product, contract and product type.
            m_Instr.Exchange = "CME";
            m_Instr.Product = "ES";
            m_Instr.Contract = "Sep12";
            m_Instr.ProdType = "FUTURE";
            // Open m_Instr.
            m_Instr.Open(true);

            // Create a new OrderSetClass object.
            m_OrderSet = new OrderSetClass();
            // Set the limits accordingly. If any of these limits is reached,
            // trading through the API will be shut down automatically.
            m_OrderSet.set_Set("MAXORDERS", 1000);
            m_OrderSet.set_Set("MAXORDERQTY", 1000);
            m_OrderSet.set_Set("MAXWORKING", 1000);
            m_OrderSet.set_Set("MAXPOSITION", 1000);
            // Enable deleting of orders. Enable the OnOrderFillData event. Enable order sending.
            m_OrderSet.EnableOrderAutoDelete = true;
            m_OrderSet.EnableOrderFillData = true;
            m_OrderSet.EnableOrderSend = true;
            // Subscribe to the OnOrderFillData event.
            m_OrderSet.OnOrderFillData += new OrderSetClass.OnOrderFillDataEventHandler(OnOrderFillData);
            // Open the m_OrderSet.
            m_OrderSet.Open(true);
            // Associate m_OrderSet with m_Instr.
            m_Instr.OrderSet = m_OrderSet;
        }

        private void OnNotifyUpdate(InstrNotifyClass pNotify, InstrObjClass pInstr)
        {
            Tick m_Tick = new Tick(DateTime.Now, Convert.ToDouble(pInstr.get_Get("LAST")), Convert.ToDouble(pInstr.get_Get("LASTQTY")));
            OnInstrumentUpdate(m_Tick);
        }

        public bool EnterOrder(string m_BS, double m_Qty, string m_FFT)
        {
            try
            {
                OrderProfileClass m_Profile = new OrderProfileClass();
                m_Profile.Instrument = m_Instr;
                m_Profile.set_Set("ACCT", "12345");
                m_Profile.set_Set("BUYSELL", m_BS);
                m_Profile.set_Set("ORDERTYPE", "M");
                m_Profile.set_Set("ORDERQTY", m_Qty.ToString());
                m_Profile.set_Set("FFT", m_FFT);
                long myResult = m_OrderSet.SendOrder(m_Profile);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private void OnOrderFillData(FillObj m_Fill)
        {
            
            OnInstrumentFill(Convert.ToInt32(m_Fill.get_Get("QTY")),
                    Convert.ToString(m_Fill.get_Get("BUYSELL")),
                    Convert.ToString(m_Fill.get_Get("PRICE")),
                    Convert.ToString(m_Fill.get_Get("KEY")));
        }
        public double Bid
        {
            get { return Convert.ToDouble(m_Instr.get_Get("BID")); }
        }
        public double Ask
        {
            get { return Convert.ToDouble(m_Instr.get_Get("ASK")); }
        }

        public double TickSize()
        {
            return m_Instr.TickSize;
        }

        public void ShutDown()
        {
            m_Notify.OnNotifyUpdate -= new InstrNotifyClass.OnNotifyUpdateEventHandler(OnNotifyUpdate);
            m_OrderSet.OnOrderFillData -= new OrderSetClass.OnOrderFillDataEventHandler(OnOrderFillData);
            m_Notify = null;
            m_Instr = null;
            m_OrderSet = null;
        }
    }
}