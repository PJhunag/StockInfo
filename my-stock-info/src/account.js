import axios from 'axios';
import React, { Component } from 'react';
import { withStyles } from "@material-ui/core/styles";
import Button from '@material-ui/core/Button';
import Dialog from '@material-ui/core/Dialog';
import DialogActions from '@material-ui/core/DialogActions';
import DialogTitle from '@material-ui/core/DialogTitle';

//var account = require('./account.js')
var CryptoJS = require("crypto-js");

var url_server = 'http://localhost:8000/'; //server ip

//使用者資訊
var user_info = {
    logined: false, //是否已經登入
    id: "", //帳號
}

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
});

//取得我的最愛
async function favorite(acc) {
    //註冊驗證
    var url = url_server + "favorite";
    console.log("url:" + url)

    var infos = { "account": acc };
    var msg = { "msg": await encoder(infos) };

    var res_msg;

    try {
        var response = await axios.post(url, msg);
        for (var i = 0; i < response.data.length; i++) {
            res_msg[i] = {
                "id": response.data.id,        //股票代碼
                "desc": response.data.desc,    //股票說明
                "price": response.data.price,  //當日收盤價
                "amp": response.data.amp,      //當日漲幅(值)
                "percent": response.data.amp   //當日漲幅(%)
            }
        }
        var s_res_msg = JSON.stringify(res_msg)
    }
    catch (e) {
        console.log(e); // Network Error
        console.log(e.status); // undefined
        console.log(e.code); // undefined
    }
    return s_res_msg;
}

//加密用
async function encoder(infos) {
    var plaintText = JSON.stringify(infos);
    var keyStr = "ka0132oftreeNode"

    // 字符串类型的key用之前需要用uft8先parse一下才能用
    var key = CryptoJS.enc.Utf8.parse(keyStr);

    // 加密
    var encryptedData = CryptoJS.AES.encrypt(plaintText, key, {
        mode: CryptoJS.mode.ECB,
        padding: CryptoJS.pad.Pkcs7
    });

    var encryptedBase64Str = encryptedData.toString();
    // 输出：'RJcecVhTqCHHnlibzTypzuDvG8kjWC+ot8JuxWVdLgY='
    console.log(encryptedBase64Str);

    // 需要读取encryptedData上的ciphertext.toString()才能拿到跟Java一样的密文
    var encryptedStr = encryptedData.ciphertext.toString();
    // 输出：'44971e715853a821c79e589bcd3ca9cee0ef1bc923582fa8b7c26ec5655d2e06'
    console.log(encryptedStr);

    // 拿到字符串类型的密文需要先将其用Hex方法parse一下
    var encryptedHexStr = CryptoJS.enc.Hex.parse(encryptedStr);

    // 将密文转为Base64的字符串
    // 只有Base64类型的字符串密文才能对其进行解密
    encryptedBase64Str = CryptoJS.enc.Base64.stringify(encryptedHexStr);

    return encryptedBase64Str;

}

class Account extends Component {
    constructor(props) {
        super(props)
        this.state = {
            id: 'None',
            name: 'None',
            login_open: false, //跳窗登入
            msg_open: false, //訊息顯示
            msg: "", //提示用訊息變數
            register_open: false, //跳窗註冊
            login_disable: false, //是否顯示login按鈕
            logout_disable: true, //是否顯示logout按鈕
        }

        //先判斷是否已登入
        var search = window.location.search;
        var params = new URLSearchParams(search);
        var user_id = params.get('id');
        var user_name = params.get('name');

        this.state.id = user_id;
        this.state.name = user_name;
        console.log("this.state.name:"+this.state.name);
        if (this.state.name != "None" && this.state.name != null ) {
            //代表已經登入
            //重整login logout按鈕
            this.setState(state => ({ login_disable: false, logout_disable: true }));
            user_info.id = user_id;
            user_info.logined = true;
            //回傳使用者資訊給上層
            //this.props.setUser(this.state.id, this.state.user);
        }
    }

    //開啟登入窗
    login_open = () => {
        this.setState(state => ({ login_open: true }));
    };

    //放棄登入
    login_cancel = () => {
        this.setState(state => ({ login_open: false }));
    };

    //開啟登入窗(google)
    register_open_google = () => {
        //開啟google第三方登入
        // for google oauth
        var google_client_id = "1043116507129-flcmle4rt4a4u9mdr11gf3eme7jrv86r.apps.googleusercontent.com";
        var google_callback_url = "http://mystockbyth.ddns.net:5000/googleLogin/callback";
        var google_oauth_url = "https://accounts.google.com/o/oauth2/v2/auth?" +
            //Scope可以參考文件裡各式各樣的scope，可以貼scope url或是個別命名
            "scope=email%20profile&" +
            "redirect_uri=" + google_callback_url + "&" +
            "response_type=code&" +
            "client_id=" + google_client_id;

        window.location.replace(google_oauth_url)

    }

    //開啟登入窗(google)
    register_open_facebook = () => {
        //開啟google第三方登入
        // for google oauth
        var facebook_client_id = "1129004447267178";
        var facebook_oauth_url = "https://www.facebook.com/dialog/oauth?redirect_uri=" +
            "http://localhost:8000/facebook_login/callback&" +
            "client_id=" + facebook_client_id + "&scope=public_profile&response_type=code";

        window.location.replace(facebook_oauth_url)
    };

    //登出
    logout = () => {
        //紀錄登入狀態與資訊
        user_info.logined = false; //未登入
        user_info.id = "None";

        //重整login logout按鈕
        this.setState(state => ({ login_disable: false, logout_disable: true }));

        //回傳使用者資訊給上層
        //this.props.setUser("None");
    };

    render() {
        return (
            <div className='login'>
                {user_info.logined && (
                    <div style={{ display: "flex" }}>
                        <Button color="inherit" size="small">歡迎 {this.state.name}</Button>
                        <Button color="inherit" disabled={this.state.login_disable} onClick={this.logout} style={{ marginRight: "auto" }}>LogOut</Button>
                    </div>
                )}
                {!user_info.logined && (
                    <div style={{ display: "flex" }}>
                        <Button color="inherit" disabled={this.state.login_disable} onClick={this.login_open} style={{ marginRight: "auto" }}>Login</Button>
                    </div>
                )}
                <div>
                    <Dialog open={this.state.login_open} onClose={this.login_cancel} aria-labelledby="form-dialog-title" >
                        <DialogTitle id="form-dialog-title">請選擇登入方式:</DialogTitle>
                        <DialogActions>
                            <Button onClick={this.register_open_google} color="primary">登入(GOOGLE)</Button>
                            <Button onClick={this.register_open_facebook} color="primary">登入(FACEBOOK)</Button>
                            <Button onClick={this.login_cancel} color="primary">離開</Button>
                        </DialogActions>
                    </Dialog>
                </div>
            </div >
        )
    }
}
export default withStyles(styles)(Account);