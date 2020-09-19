using System;

//epsl_t : EPS紀錄
public class epsl {
    public string epsl001 { get; set; } //股票編號
    public int epsl002 { get; set; } //年度
    public string epsl003 { get; set; } //區間
    public double? epsl004 { get; set; } //eps
}

//股價資訊(上市股票)
public class twse {
    public string twse001 { get; set; } //股票編號
    public DateTime twse002 { get; set; } //日期
    public string twse003 { get; set; } //成交股數
    public string twse004 { get; set; } //開盤價
    public string twse005 { get; set; } //最高價
    public string twse006 { get; set; } //最低價
    public string twse007 { get; set; } //收盤價
    public string twse008 { get; set; } //價差
    public string twse009 { get; set; } //本益比
}

//股價資訊(上櫃股票)
public class tpex {
    public string tpex001 { get; set; } //股票編號
    public DateTime tpex002 { get; set; } //日期
    public string tpex003 { get; set; } //成交股數
    public string tpex004 { get; set; } //開盤價
    public string tpex005 { get; set; } //最高價
    public string tpex006 { get; set; } //最低價
    public string tpex007 { get; set; } //收盤價
    public string tpex008 { get; set; } //價差
    public string tpex009 { get; set; } //本益比
}

//月營收紀錄
public class opme {
    public string opme001 { get; set; } //股票代碼
    public int opme002 { get; set; } //年度  
    public int opme003 { get; set; } //月份 
    public Double opme004 { get; set; } //單月營收	
    public Double opme005 { get; set; } //去年同月營收	
    public Double opme006 { get; set; } //單月月增率	
    public Double opme007 { get; set; } //單月年增率	
    public Double opme008 { get; set; } //累計營收	
    public Double opme009 { get; set; } //去年累計營收	
    public Double opme010 { get; set; } //累積年增率   
}

//歷年損益表
public class prft {
    public string prft001 { get; set; } //股票代碼
    public int prft002 { get; set; } //年度  
    public string prft003 { get; set; } //季別 
    public int prft004 { get; set; } //營收	
    public int prft005 { get; set; } //毛利	
    public int prft006 { get; set; } //營業利益
    public int prft007 { get; set; } //稅前淨利
    public int prft008 { get; set; } //稅後淨利
}

//歷年股利紀錄
public class divd {
    public string divd001 { get; set; } //股票代碼
    public int? divd002 { get; set; } //發放年度	
    public string? divd003 { get; set; } //除權日	
    public string? divd004 { get; set; } //除息日	 
    public double? divd005 { get; set; } //除權息前股價
    public double? divd006 { get; set; } //股票股利	
    public double? divd007 { get; set; } //現金股利	  
    public double? divd008 { get; set; } //EPS	  
    public double? divd009 { get; set; } //配息率	
    public double? divd010 { get; set; } //現金殖利率
}

public class KD {
    public string no { get; set; } //編號	
    public DateTime dt { get; set; } //日期
    public double RSV { get; set; }
    public double K { get; set; }
    public double D { get; set; }
}

public class YearLine {
    public string no { get; set; } //編號	
    public DateTime dt { get; set; } //日期
    public double yearLine { get; set; }
}