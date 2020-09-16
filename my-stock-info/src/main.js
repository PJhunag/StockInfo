import React, { Component } from 'react';
import StockCharts from './showStockCharts'; //股票區塊(圖表)
import StockTables from './showStockTables'; //股票區塊(表格)
import StockEvaluations from './showEvaluations'; //股票區塊(表格)
import Account from './account'; //帳號區塊

//material-ui
import { withStyles } from "@material-ui/core/styles";
import SearchIcon from '@material-ui/icons/Search';
import FavoriteIcon from '@material-ui/icons/FavoriteBorder';
import HistoryIcon from '@material-ui/icons/History';
import Typography from '@material-ui/core/Typography';
import ListItem from '@material-ui/core/ListItem';
import ListItemIcon from '@material-ui/core/ListItemIcon';
import ListItemText from '@material-ui/core/ListItemText';
import ExpandLess from '@material-ui/icons/ExpandLess';
import ExpandMore from '@material-ui/icons/ExpandMore';
import Collapse from '@material-ui/core/Collapse';
import List from '@material-ui/core/List';
import Toolbar from '@material-ui/core/Toolbar';
import AppBar from '@material-ui/core/AppBar';
import Headroom from 'react-headroom';

//for select區塊追加
import Select from 'react-select';
import { emphasize } from '@material-ui/core/styles/colorManipulator';
import purple from '@material-ui/core/colors/purple';
import TextField from '@material-ui/core/TextField';
import MenuItem from '@material-ui/core/MenuItem';
import Chip from '@material-ui/core/Chip';
import classNames from 'classnames';
import FormControlLabel from '@material-ui/core/FormControlLabel';
import FavoriteBorder from '@material-ui/icons/FavoriteBorder';
import Checkbox from '@material-ui/core/Checkbox';
import Favorite from '@material-ui/icons/Favorite';
import axios from 'axios';

//警告跳窗用
import Button from '@material-ui/core/Button';
import Dialog from '@material-ui/core/Dialog';
import DialogActions from '@material-ui/core/DialogActions';
import DialogContent from '@material-ui/core/DialogContent';
import DialogContentText from '@material-ui/core/DialogContentText';
import DialogTitle from '@material-ui/core/DialogTitle';


//先準備相關資訊
var instance = axios.create({
  baseURL: 'http://114.33.59.86:5000'
});
var getFavotie = require('./favorite.js')

//取得股票清單
var stock_list = [];
var stock_no = "未選擇"; //股票代碼
var stock_desc = "未選擇"; //股票說明


const styles = theme => ({

  input: {
    display: 'flex',
    padding: 0,
  },
  valueContainer: {
    display: 'flex',
    flex: 1,
    alignItems: 'center',
  },
  chip: {
    margin: `${theme.spacing.unit / 2}px ${theme.spacing.unit / 4}px`,
  },
  chipFocused: {
    backgroundColor: emphasize(
      theme.palette.type === 'light' ? theme.palette.grey[300] : theme.palette.grey[700],
      0.08,
    ),
  },
  noOptionsMessage: {
    fontSize: 16,
    padding: `${theme.spacing.unit}px ${theme.spacing.unit * 2}px`,
  },
  singleValue: {
    fontSize: 16,
  },
  placeholder: {
    position: 'absolute',
    left: 2,
    fontSize: 16,
  },
  margin: {
    margin: theme.spacing.unit
  },
  search: {
    color: theme.palette.getContrastText(purple[200]),
    backgroundColor: purple[200],
    "&:hover": {
      backgroundColor: purple[400]
    },
    width: '90%',
    marginTop: '5%',
    marginLeft: '5%',
    marginright: '5%'
  },
  flex: {
    flexGrow: 1
  }
});
function NoOptionsMessage(props) {
  return (
    <Typography
      color="textSecondary"
      className={props.selectProps.classes.noOptionsMessage}
      {...props.innerProps}
    >
      {props.children}
    </Typography>
  );
}

function inputComponent({ inputRef, ...props }) {
  return <div ref={inputRef} {...props} />;
}

function Control(props) {
  return (
    <TextField
      fullWidth
      InputProps={{
        inputComponent,
        inputProps: {
          className: props.selectProps.classes.input,
          ref: props.innerRef,
          children: props.children,
          ...props.innerProps,
        },
      }}
    />
  );
}

function Option(props) {
  return (
    <MenuItem
      buttonRef={props.innerRef}
      selected={props.isFocused}
      component="div"
      style={{
        fontWeight: props.isSelected ? 500 : 400,
      }}
      {...props.innerProps}
    >
      {props.children}
    </MenuItem>
  );
}

function Placeholder(props) {
  return (
    <Typography
      color="textSecondary"
      className={props.selectProps.classes.placeholder}
      {...props.innerProps}
    >
      {props.children}
    </Typography>
  );
}

function SingleValue(props) {
  return (
    <Typography className={props.selectProps.classes.singleValue} {...props.innerProps}>
      {props.children}
    </Typography>
  );
}

function ValueContainer(props) {
  return <div className={props.selectProps.classes.valueContainer}>{props.children}</div>;
}
function MultiValue(props) {
  return (
    <Chip
      tabIndex={-1}
      label={props.children}
      className={classNames(props.selectProps.classes.chip, {
        [props.selectProps.classes.chipFocused]: props.isFocused,
      })}
      onDelete={event => {
        props.removeProps.onClick();
        props.removeProps.onMouseDown(event);
      }}
    />
  );
}

const components = {
  Option,
  Control,
  NoOptionsMessage,
  Placeholder,
  SingleValue,
  MultiValue,
  ValueContainer,
};

var getFvrt = require('./favorite.js');


class Main extends Component {
  constructor() {
    super()

    var ls_stock_list = new Array()
    var url = "/getStockList/0";

    this.state = {
      open: true,
      open2: false,
      open3: false,
      choice_name: 'getStockPrices', //stockPrices,tradingVolume
      choice_desc: '歷史收盤價',
      user: "None", //使用者ID,
      show_stockCharts: true, //顯示股票頁(圖表)
      show_stockTables: false, //顯示股票頁(表格)
      show_stockEvaluations: false, //顯示評估頁
      show_favorite: false, //顯示我的最愛
      favorite: <div>favorite</div>,
      favorited: false,
      stock_list: ls_stock_list,
      no: "",
      desc: "",
      show_title: true, //顯示title,
      alertOpne: false, //錯誤警示
      id: "", //使用者ID(帳號)
      name: "", //用戶名稱
      show_StockNo: true, //顯示股票代碼及我的最愛
      show_StockSelect: true, //顯示股票挑選區塊
    }
    this.handleChange_getMyFavorite = this.handleChange_getMyFavorite.bind(this); //favorite
    this.handleChange_fromFavoriteToStockPrices = this.handleChange_fromFavoriteToStockPrices.bind(this); //click favorite
    this.handleChange_wheel = this.handleChange_wheel.bind(this); //wheel
    this.handleChange_no_edit = this.handleChange_no_edit.bind(this); //edit no
    this.handleChange_no = this.handleChange_no.bind(this); //choice no

    //先判斷是否已登入
    var search = window.location.search;
    var params = new URLSearchParams(search);
    var user_id = params.get('id');
    var user_name = params.get('name');

    this.state.id = user_id;
    this.state.name = user_name;
    this.setState(state => ({ choice_name: 'None', show_favorite: false, }));

    this.handleChange_no_edit("0");

  }

  //給予標題
  componentDidMount() {
    document.title = "我的股票資訊分析"
  }


  //錯誤警示
  alertClose = () => {
    console.log("alertOpne:" + this.state.alertOpne);
    this.setState(state => ({ alertOpne: !state.alertOpne }));
  };

  //下拉展開(股票資訊)
  expand_option = () => {
    this.setState(state => ({ open: !state.open }));
  };

  //下拉展開(我的最愛)
  expand_option2 = () => {
    this.setState(state => ({ open2: !state.open2 }));
  };

  //下拉展開(瀏覽紀錄)
  expand_option3 = () => {
    this.setState(state => ({ open3: !state.open3 }));
  };

  handleChange_getStockPrices = () => {
    //呼叫並且刷新主要區塊(右下)
    if (stock_no == "None") {
      this.setState(state => ({ alertOpne: !state.alertOpne }));
    }
    else {
      console.log("stock_no:" + stock_no)
      this.setState(state => ({ choice_name: 'getStockPrices', choice_desc: '歷史收盤價' }));
      this.setState(state => ({ show_stockCharts: true, show_stockTables: false, show_favorite: false, show_stockEvaluations: false }));
    }

    //show_StockSelect : 挑選股票
    //show_StockNo     : 股票號碼及說明
    this.setState(state => ({ show_StockNo: true, show_StockSelect: true }));
  }

  handleChange_getTradingVolume = () => {
    //呼叫並且刷新主要區塊(右下)
    console.log("stock_no:" + stock_no)
    if (stock_no == "None") {
      this.setState(state => ({ alertOpne: !state.alertOpne }));
    }
    else {
      this.setState(state => ({ choice_name: 'getTradingVolume', choice_desc: '歷史成交量' }));
      this.setState(state => ({ show_stockCharts: true, show_stockTables: false, show_favorite: false, show_stockEvaluations: false }));
    }
  }

  //歷史EPS
  handleChange_getEPS = () => {
    //呼叫並且刷新主要區塊(右下)
    console.log("stock_no:" + stock_no)
    if (stock_no == "None") {
      this.setState(state => ({ alertOpne: !state.alertOpne }));
    }
    else {
      console.log("stock_no:" + stock_no)
      this.setState(state => ({ choice_name: 'getEPS', choice_desc: '歷史EPS' }));
      this.setState(state => ({ show_stockCharts: true, show_stockTables: false, show_favorite: false, show_stockEvaluations: false }));
    }

    //show_StockSelect : 挑選股票
    //show_StockNo     : 股票號碼及說明
    this.setState(state => ({ show_StockNo: true, show_StockSelect: true }));
  }

  //歷史月營收
  handleChange_getOperatingIncomes = () => {
    //呼叫並且刷新主要區塊(右下)
    console.log("stock_no:" + stock_no)
    if (stock_no == "None") {
      this.setState(state => ({ alertOpne: !state.alertOpne }));
    }
    else {
      console.log("choice_name:" + this.state.choice_name);
      this.state.choice_name = 'getOperatingIncomes'
      this.setState(state => ({ choice_name: 'getOperatingIncomes', choice_desc: '歷史月營收', show_favorite: false, no: stock_no }));
      this.setState(state => ({ show_stockCharts: false, show_stockTables: true, show_favorite: false, show_stockEvaluations: false }));
    }

    //show_StockSelect : 挑選股票
    //show_StockNo     : 股票號碼及說明
    this.setState(state => ({ show_StockNo: true, show_StockSelect: true }));
  }

  handleChange_getDividend = () => {
    //呼叫並且刷新主要區塊(右下)
    console.log("stock_no:" + stock_no)
    if (stock_no == "None") {
      this.setState(state => ({ alertOpne: !state.alertOpne }));
    }
    else {
      console.log("choice_name:" + this.state.choice_name);
      this.state.choice_name = 'getDividend'
      this.setState(state => ({ choice_name: 'getDividend', choice_desc: '歷史股息股利', show_favorite: false, no: stock_no, }));
      this.setState(state => ({ show_stockCharts: false, show_stockTables: true, show_favorite: false, show_stockEvaluations: false }));
    }

    //show_StockSelect : 挑選股票
    //show_StockNo     : 股票號碼及說明
    this.setState(state => ({ show_StockNo: true, show_StockSelect: true }));
  }

  handleChange_getProfit = () => {
    //呼叫並且刷新主要區塊(右下)
    console.log("stock_no:" + stock_no)
    if (stock_no == "None") {
      this.setState(state => ({ alertOpne: !state.alertOpne }));
    }
    else {
      console.log("choice_name:" + this.state.choice_name);
      this.state.choice_name = 'getProfit'
      this.setState(state => ({ choice_name: 'getProfit', choice_desc: '歷史損益', show_favorite: false, no: stock_no, }));
      this.setState(state => ({ show_stockCharts: false, show_stockTables: true, show_favorite: false, show_stockEvaluations: false }));
    }

    //show_StockSelect : 挑選股票
    //show_StockNo     : 股票號碼及說明
    this.setState(state => ({ show_StockNo: true, show_StockSelect: true }));
  }

  handleChange_getEvaluation = () => {
    //呼叫並且刷新主要區塊(右下)
    console.log("stock_no:" + stock_no)
    if (stock_no == "None") {
      this.setState(state => ({ alertOpne: !state.alertOpne }));
    }
    else {
      console.log("choice_name:" + this.state.choice_name);
      this.state.choice_name = 'getProfit'
      this.setState(state => ({ choice_name: 'getEvaluations', choice_desc: '整體評估', show_favorite: false, no: stock_no, }));
      this.setState(state => ({ show_stockCharts: false, show_stockTables: false, show_favorite: false, show_stockEvaluations: true }));
    }

    //show_StockSelect : 挑選股票
    //show_StockNo     : 股票號碼及說明
    this.setState(state => ({ show_StockNo: true, show_StockSelect: true }));
  }

  async handleChange_getMyFavorite() {
    //呼叫並且刷新主要區塊(右下)
    //顯示我的最愛區塊
    //隱藏其他區塊
    var fvrt = await getFvrt.getFavoriteCards(this.state.user, this.handleChange_fromFavoriteToStockPrices);
    this.setState(state => ({ show_stockCharts: false, show_stockTables: false, show_favorite: true, favorite: fvrt }));

    //show_StockSelect : 挑選股票
    //show_StockNo     : 股票號碼及說明
    this.setState(state => ({ show_StockNo: false, show_StockSelect: false, show_stockEvaluations: false }));

  }



  //從我的最愛挑選要顯示資訊的股票
  handleChange_fromFavoriteToStockPrices(stock) {

    // console.log('即將開啟(' + no+')股票資訊!')
    //指定no
    this.setState(state => ({ no: stock.id }));
    this.setState(state => ({ desc: stock.name }));
    //刷新股票資訊
    this.handleChange_getStockPrices();
  }

  //設置使用者
  setUser = user_id => {
    this.setState(state => ({ user: user_id }));
    console.log("this.state.user:" + this.state.user)
  }

  //控制是否顯示title
  handleChange_wheel(event) {
    /*     var delta = event.deltaY;
        console.log(delta)
        if (delta == "100") {
          //往下滾動, 隱藏選單
          //this.setState(state => ({ show_title:false}));
          $('title').removeClass('header-down').addClass('header-up');
        }
        else {
          //往上滾動, 顯示選單
          //this.setState(state => ({ show_title:true}));
          $('title').removeClass('header-up').addClass('header-down');
        } */

    var header = new Headroom(document.querySelector("#header"), {
      tolerance: 5,
      offset: 205,
      classes: {
        initial: "animated",
        pinned: "slideDown",
        unpinned: "slideUp"
      }
    });
    header.init();

    var bttHeadroom = new Headroom(document.getElementById("btt"), {
      tolerance: 0,
      offset: 500,
      classes: {
        initial: "slide",
        pinned: "slide--reset",
        unpinned: "slide--down"
      }
    });
    bttHeadroom.init();
  }

  //挑選股票代碼(編輯過程中)
  handleChange_no_edit(value) {
    var url = "/getStockList/" + value;
    instance.get(url).then(response => {
      var data = response.data;
      var ls_stock_list = new Array()
      //console.log("edit:" + data);

      for (var i = 0; i < data.length; i++) {
        //console.log(data[i].id + "(" + data[i].name + ")");
        ls_stock_list[i] = { label: data[i].id + "(" + data[i].name + ")" };
      }
      stock_list = ls_stock_list;
      this.forceUpdate();
    });
  }

  //挑選股票代碼(完成挑選後)
  async handleChange_no(event) {
    // event.target 是當前的 DOM elment
    // 從 event.target.value 取得 user 剛輸入的值
    // 將 user 輸入的值更新回 state
    stock_no = event.label;
    stock_desc = stock_no.substr(stock_no.indexOf("(") + 1, stock_no.indexOf(")") - stock_no.indexOf("(") - 1)
    stock_no = stock_no.substr(0, stock_no.indexOf("("))
    console.log("stock_desc:" + stock_no + "-")
    console.log("stock_desc:" + stock_desc + "-")
    //StockCharts.handleChange_no(event);
    this.setState(state => ({ no: stock_no, desc: stock_desc }));
    //this.handleChange_show_chart(this.state.choice_name); //顯示圖表
    var chkfvrt = await getFavotie.checkFavorite(stock_no);
    //this.state.favorited = chkfvrt; //檢核是否已加入我的最愛
    //console.log("this.state.favorited:" + this.state.favorited)
    //console.log("chkfcrt:" + chkfvrt + ",type" + typeof (chkfvrt))
    console.log("我的最愛 " + stock_no + " 確認狀態為 " + chkfvrt + " 並進行更新")
    this.setState(state => ({ favorited: chkfvrt }));
  }

  //改變收藏狀態
  setFavorite = (event, checked) => {

    console.log("添加我的最愛:" + this.state.no);
    if (this.props.user == "None") {
      //判斷是否已登入
      this.setState(state => ({ msg_open: true, favorited: false, msg: "請先登入系統後再重新添加至我的最愛!" }));
    }
    else if (this.state.no == "") {
      //判斷是否已挑股票
      this.setState(state => ({ msg_open: true, favorited: false, msg: "請先挑選股票後再重新添加至我的最愛!" }));
    }
    else {
      //改變畫面狀態
      this.setState(state => ({ favorited: !this.state.favorited }));

      var myfavorite = require('./favorite.js')
      if (this.state.favorited == !true) {
        //呼叫添加
        myfavorite.insertFavorite(this.state.no);
      }
      else {
        //呼叫刪除
        myfavorite.deleteFavorite(this.state.no);
      }
    }
  };

  render() {
    const { classes } = this.props;
    return (
      <div >
        <Headroom>
          <div className='top' >
            <AppBar classname="title" position="static">
              <Toolbar>
                <Typography variant="title" color="inherit" className={classes.flex}>
                  <font className='title' face="微軟正黑體" size="8"><b>股票查詢</b></font><font className='title' face="微軟正黑體" size="3"><b>T.H</b></font>
                </Typography>
                <Account setUser={this.setUser} />
              </Toolbar>
            </AppBar>
          </div>
        </Headroom>
        <div className='menu'>
          <ListItem button onClick={this.expand_option}>
            <ListItemIcon>
              <SearchIcon />
            </ListItemIcon>
            <ListItemText inset primary="個股資訊" />
            {this.state.open ? <ExpandLess /> : <ExpandMore />}
          </ListItem>
          <Collapse in={this.state.open} timeout="auto" unmountOnExit>
            <List className="sub_button" component="div" disablePadding>
              <ListItem button onClick={this.handleChange_getStockPrices}>
                <ListItemIcon>
                  <SearchIcon />
                </ListItemIcon>
                <ListItemText inset primary="歷史收盤價" />
              </ListItem>
            </List>
            <List className="sub_button" component="div" disablePadding>
              <ListItem button onClick={this.handleChange_getEPS}>
                <ListItemIcon>
                  <SearchIcon />
                </ListItemIcon>
                <ListItemText inset primary="歷史EPS" />
              </ListItem>
            </List>
            <List className="sub_button" component="div" disablePadding>
              <ListItem button onClick={this.handleChange_getOperatingIncomes}>
                <ListItemIcon>
                  <SearchIcon />
                </ListItemIcon>
                <ListItemText inset primary="歷史營收" />
              </ListItem>
            </List>
            <List className="sub_button" component="div" disablePadding>
              <ListItem button onClick={this.handleChange_getDividend}>
                <ListItemIcon>
                  <SearchIcon />
                </ListItemIcon>
                <ListItemText inset primary="歷史股利" />
              </ListItem>
            </List>
            <List className="sub_button" component="div" disablePadding>
              <ListItem button onClick={this.handleChange_getProfit}>
                <ListItemIcon>
                  <SearchIcon />
                </ListItemIcon>
                <ListItemText inset primary="歷史損益" />
              </ListItem>
            </List>
            <List className="sub_button" component="div" disablePadding>
              <ListItem button onClick={this.handleChange_getEvaluation}>
                <ListItemIcon>
                  <SearchIcon />
                </ListItemIcon>
                <ListItemText inset primary="整體評估" />
              </ListItem>
            </List>
          </Collapse>

          <ListItem button onClick={this.expand_option2}>
            <ListItemIcon>
              <FavoriteIcon />
            </ListItemIcon>
            <ListItemText inset primary="我的最愛" />
            {this.state.open2 ? <ExpandLess /> : <ExpandMore />}
          </ListItem>
          <Collapse in={this.state.open2} timeout="auto" unmountOnExit>
            <List className="sub_button" component="div" disablePadding>
              <ListItem button onClick={this.handleChange_getMyFavorite}>
                <ListItemIcon>
                  <FavoriteIcon />
                </ListItemIcon>
                <ListItemText inset primary="收藏清單" />
              </ListItem>
            </List>
            <List className="sub_button" component="div" disablePadding>
              <ListItem button onClick={this.handleClick}>
                <ListItemIcon>
                  <FavoriteIcon />
                </ListItemIcon>
                <ListItemText inset primary="施工中" />
              </ListItem>
            </List>
          </Collapse>

          <ListItem button onClick={this.expand_option3}>
            <ListItemIcon>
              <HistoryIcon />
            </ListItemIcon>
            <ListItemText inset primary="瀏覽紀錄" />
            {this.state.open3 ? <ExpandLess /> : <ExpandMore />}
          </ListItem>
          <Collapse in={this.state.open3} timeout="auto" unmountOnExit>
            <List className="sub_button" component="div" disablePadding>
              <ListItem button onClick={this.handleClick}>
                <ListItemIcon>
                  <HistoryIcon />
                </ListItemIcon>
                <ListItemText inset primary="施工中" />
              </ListItem>
            </List>
            <List className="sub_button" component="div" disablePadding>
              <ListItem button onClick={this.handleClick}>
                <ListItemIcon>
                  <HistoryIcon />
                </ListItemIcon>
                <ListItemText inset primary="施工中" />
              </ListItem>
            </List>
          </Collapse>
        </div>
        <div className='parent'>
          <div className='search'>
            <div className='search'>
              <font face="微軟正黑體" size="8"><b> {this.state.choice_desc} </b></font>
              <p />
              {this.state.show_StockSelect &&
                <Select
                  classes={classes}
                  options={stock_list}
                  components={components}
                  value="{this.stock_no}"
                  onChange={this.handleChange_no}
                  placeholder="請選擇欲查詢的股票代碼"
                  autoWidth="true"
                  onInputChange={this.handleChange_no_edit}
                  native="true"
                />}
              <p />
              {this.state.show_StockNo &&
                <p>
                  <font face="微軟正黑體" size="6">目前顯示的股票代碼為：{stock_no} </font>
                  <font face="微軟正黑體" size="4">({stock_desc}) &nbsp;&nbsp;&nbsp;&nbsp;</font>
                  <FormControlLabel control={<Checkbox icon={<FavoriteBorder />} checkedIcon={<Favorite />} checked={this.state.favorited} />}
                    label="收藏" onChange={this.setFavorite} />
                </p>}
              {this.state.show_stockCharts && <StockCharts name={this.state.choice_name} desc={this.state.choice_desc} user={this.state.user} no={this.state.no} desc={this.state.desc} />}
              {this.state.show_stockTables && <StockTables name={this.state.choice_name} desc={this.state.choice_desc} user={this.state.user} no={this.state.no} desc={this.state.desc} />}
              {this.state.show_stockEvaluations && <StockEvaluations name={this.state.choice_name} desc={this.state.choice_desc} user={this.state.user} no={this.state.no} desc={this.state.desc} />}
              {this.state.show_favorite && this.state.favorite}
            </div>
          </div>
        </div>
        <Dialog
          open={this.state.alertOpne}
          onClose={this.alertClose}
          aria-labelledby="alert-dialog-title"
          aria-describedby="alert-dialog-description"
        >
          <DialogTitle id="alert-dialog-title">{"[尚未選擇指定股票]"}</DialogTitle>
          <DialogContent>
            <DialogContentText id="alert-dialog-description">
              請先挑選欲查詢之股票編號!
          </DialogContentText>
          </DialogContent>
          <DialogActions>
            <Button onClick={this.alertClose} color="primary">
              離開
          </Button>
          </DialogActions>
        </Dialog>
      </div>
    )
  }
}
export default withStyles(styles)(Main); 