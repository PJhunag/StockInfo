using System;
public class Stock {
    public string id { get; set; }
    public string name { get; set; }
    public string price { get; set; }
    public string fluct { get; set; }
    public string percent { get; set; }
}
public class StockPrice {
    public string date { get; set; }

    public double? price { get; set; }

}

public class Message {
    public string code { get; set; }

    public string message { get; set; }

}

//月營收紀錄
public class StockOperatingIncomes {
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

public class StockDividend {
    public int? divd002 { get; set; } //發放年度	
    public string? divd003 { get; set; } //除權日	
    public string? divd004 { get; set; } //除息日	 
    public string? divd005 { get; set; } //除權息前股價
    public string? divd006 { get; set; } //股票股利	
    public string? divd007 { get; set; } //現金股利	  
    public string? divd008 { get; set; } //EPS	  
    public string? divd009 { get; set; } //配息率	
    public string? divd010 { get; set; } //現金殖利率

}

public class StockProfit {
    public int prft002 { get; set; } //年度  
    public string prft003 { get; set; } //季別 
    public string prft004 { get; set; } //營收	
    public string prft005 { get; set; } //毛利	
    public string prft006 { get; set; } //營業利益
    public string prft007 { get; set; } //稅前淨利
    public string prft008 { get; set; } //稅後淨利
}

public class StockEPS {
    public int year { get; set; }

    public string season { get; set; }

    public string eps { get; set; }

}

public class GoogleAccessToken {
    public string access_token { get; set; }

    public string expires_in { get; set; }

    public string scope { get; set; }

    public string token_type { get; set; }

    public string id_token { get; set; }

}

public class GoogleUserInfo {
    public string id { get; set; }
    public string email { get; set; }
    public string verified_email { get; set; }
    public string name { get; set; }
    public string given_name { get; set; }
    public string family_name { get; set; }
    public string picture { get; set; }
    public string locale { get; set; }
}

public class action {
    public string type { get; set; }

    public string id { get; set; }

    public string stock { get; set; }

}

public class Evaluation {
    public string stockNo { get; set; } //股票編號
    public string stockName { get; set; } //股票名稱
    public string Price { get; set; } //目前價格
    public string Type { get; set; } //股票類別
    public string LastEPS { get; set; } //去年EPS
    public string FiveYearsAvgerageEPS { get; set; } //五年平均EPS
    public string LastFourEPS { get; set; } //近四季EPS
    public string preEPS { get; set; } //預測EPS
    public string preEPSRate { get; set; } //預測本益比
    public string LastMoneyDividend { get; set; } //去年股利
    public string LastStockDividend { get; set; } //去年股息
    public string? RevenueIncreaseRatio { get; set; } //發放率
    public string preDividend { get; set; } //預測股利
    public string FiveYearsAvgerageDividend { get; set; } //五年平均殖利率
    public string preDividendRadio { get; set; } //預測殖利率
    public string goodPrice { get; set; } //合理價位
    public string FiveYearsAvgeragePrice { get; set; } //五年均價
    public string? OperatingUpdateTime { get; set; } //營收更新月
    public string? ThisYearAccumulationOperatingIncome { get; set; } //當年營收
    public string? ThisYearAccumulationProfit { get; set; } //當年淨利
    public string? LastYearAccumulationOperatingIncome { get; set; } //去年營收
    public string? LastYearAccumulationProfit { get; set; } //去年淨利
    public string UpdateDate { get; set; } //最後更新日
    public string EPSUseRatio { get; set; } //近五年EPS發放率
    public string StockIncreaseRatio { get; set; } //股票增加比例(除權)

}