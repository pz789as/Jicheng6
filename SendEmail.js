/**
 * Sample React Native App
 * https://github.com/facebook/react-native
 * @flow
 */

import React, { Component } from 'react';
import {
    AppRegistry,
    StyleSheet,
    Text,
    View,
    TouchableOpacity,
    Alert,
    TextInput,
    PixelRatio,
    Linking,
} from 'react-native';

import Communications from 'react-native-communications';
var Mailer = require('NativeModules').RNMail;

export default class SendEmail extends Component {
    constructor(props) {
        super(props);
        this.sendNumber = '';
        this.state = {
            emailPath: 'pz789asdf@163.com',
            verifyCode: '',
            blnUpdate: false,
            blnShowVerify: false,
        };
        this.isConnected = false;
        this.tempServerData = null;
        this.connectServer();
    }
    setUpdate() {
        this.setState({
            blnUpdate: !this.blnUpdate,
        });
    }
    componentDidMount() {

    }
    componentWillUnmount() {

    }
    connectServer() {
        this.client = new WebSocket('http://192.168.1.112:8888');
        this.client.onopen = this.onConnected.bind(this);
        this.client.onerror = (e) => {
            console.log('error:', e.message);
            Alert.alert(
                'Alert',
                '网络错误，请稍后再试！' + e.message,
                [
                    { text: 'Cancel', onPress: () => console.log('Cancel Pressed!') },
                    { text: 'OK', onPress: () => console.log('OK Pressed!') },
                ]
            );
        };
        this.client.onclose = this.onClosed.bind(this);
        this.client.onmessage = this.getDataFromServer.bind(this);
    }
    onConnected() {//回调，说明链接成功
        console.log('is connected');
        this.isConnected = true;
    }
    onClosed(e) {
        console.log('code:' + e.code, 'reason:' + e.reason);
        if (e.code == 1001) {//中途服务器出问题而关闭
            Alert.alert(
                'Alert',
                '网络错误，请稍后再试！' + e.reason,
                [
                    { text: 'Cancel', onPress: () => console.log('Cancel Pressed!') },
                    { text: 'OK', onPress: () => console.log('OK Pressed!') },
                ]
            );
        }
    }
    getDataFromServer(e) {
        var data = e.data;
        var serverData = data.toString();
        console.log(serverData);
        if (this.tempServerData == null) {
            this.tempServerData = serverData;
        } else {
            this.tempServerData += serverData;
        }
        if (serverData.charAt(serverData.length - 1) == '|'){
            this.tempServerData = this.tempServerData.substr(0, this.tempServerData.length - 1);
            var serverJson = JSON.parse(this.tempServerData);
            if (serverJson.code == 0){
                Alert.alert(
                    '提示',
                    '发送失败！' + serverJson.desc,
                    [
                        { text: 'Cancel', onPress: () => console.log('Cancel Pressed!') },
                        { text: 'OK', onPress: () => console.log('OK Pressed!') },
                    ]
                );
            }else if (serverJson.code == 1) {
                this.setState({
                    blnShowVerify: true,
                });
                this.sendNumber = serverJson.desc;
                Alert.alert(
                    '提示',
                    '发送成功！',
                    [
                        { text: 'OK', onPress: () => console.log('OK Pressed!') },
                    ]
                );
            }
        }
    }
    onTextChange(text) {
        this.setState({
            emailPath: text,
        });
    }
    onSubmitSend() {
        var etips = ''
        if (this.state.emailPath == '') {
            etips = '邮箱为空！';
        } else {
            //可以判断其他情况，输入不合格的字符串之类的
        }
        if (etips != '') {
            Alert.alert(
                '提示',
                etips,
                [
                    { text: "确定", onPress: () => { } }
                ]
            );
        } else {
            if (this.isConnected) {
                this.setState({
                    blnShowVerify: false,
                });
                this.tempServerData = null;
                this.client.send(`{"code": 0, "user": "${this.state.emailPath}"}`);
            } else {
                Alert.alert(
                    '提示',
                    '未连接服务器，请重新启动！',
                    [
                        { text: "确定", onPress: () => { } }
                    ]
                );
            }
            // this.sendNumber = '';
            // for (var i = 0; i < 6; i++) {
            //     this.sendNumber += parseInt(Math.max(0, Math.random() * 10 - 1)).toString();
            // }
            // console.log(this.sendNumber);
            // Mailer.mail({
            //     subject: 'Jicheng6',
            //     recipients: [this.state.emailPath],
            //     ccRecipients: [],
            //     bccRecipients: [],
            //     body: '验证码是：' + this.sendNumber,
            //     isHTML: false,
            // }, (error, event) => {
            //     if (error) {
            //         console.log('Can\'t handle url: ' + this.state.emailPath);
            //         Alert.alert(
            //             '提示',
            //             'Can\'t handle url: ' + this.state.emailPath,
            //             [
            //                 { text: 'OK', onPress: () => { } }
            //             ]
            //         );
            //     } else {
            //         console.log('邮件发送成功，请登录您的邮箱查看验证码！');
            //         Alert.alert(
            //             '提示',
            //             '邮件发送成功，请登录您的邮箱查看验证码！',
            //             [
            //                 { text: 'OK', onPress: () => { } }
            //             ]
            //         );
            //     }
            // });
            // Communications.email([this.state.emailPath], null, null, 'Jicheng6 邮箱验证', this.sendNumber);
            // var url = 'mailto:' +
            //     this.state.emailPath +
            //     '&cc=@subject=' +
            //     'Jicheng6短信验证' +
            //     '&body=' +
            //     this.sendNumber;
            // Linking.canOpenURL(url)
            //     .then((supported) => {
            //         if (!supported) {
            //             console.log('Can\'t handle url: ' + url);
            //             Alert.alert(
            //                 '提示',
            //                 'Can\'t handle url: ' + url,
            //                 [
            //                     { text: 'OK', onPress: () => { } }
            //                 ]
            //             );
            //         } else {
            //             return Linking.openURL(url);
            //         }
            //     })
            //     .catch((err) => {
            //         console.log('An error occurred', err);
            //         Alert.alert(
            //             '提示',
            //             'An error occurred: ' + err,
            //             [
            //                 { text: 'OK', onPress: () => { } }
            //             ]
            //         );
            //     });
        }
    }
    onChangeVerifyCode(text){
        this.setState({
            verifyCode: text
        });
    }
    onSubmitOK(){
        if (this.sendNumber === this.state.verifyCode){
            Alert.alert(
                '提示',
                '认证完毕！',
                [
                    { text: "确定", onPress: () => { } }
                ]
            );
        }else{
            Alert.alert(
                '提示',
                '验证码错误，请重新输入！',
                [
                    { text: "确定", onPress: () => { } }
                ]
            );
        }
    }
    render() {
        return (
            <View style={[styles.container, this.props.style ? this.props.style : {}]}>
                <View style={styles.viewStyle}>
                    <TextInput style={styles.textInputStyle}
                        onChangeText={this.onTextChange.bind(this)}
                        value={this.state.emailPath}
                        placeholder={'请输入邮箱'}
                        onSubmitEditing={this.onSubmitSend.bind(this)} />
                    <TouchableOpacity onPress={this.onSubmitSend.bind(this)}>
                        <View style={[styles.sendButtonView, {}]}>
                            <Text style={styles.sendButtonText}>
                                发送
                            </Text>
                        </View>
                    </TouchableOpacity>
                </View>
                {!this.state.blnShowVerify ? null : 
                <View style={[styles.viewStyle, {marginTop: 5}]}>
                    <TextInput style={styles.textInputStyle}
                        onChangeText={this.onChangeVerifyCode.bind(this)}
                        value={this.state.verifyCode}
                        placeholder={'请输入验证码'}
                        onSubmitEditing={this.onSubmitOK.bind(this)} />
                    <TouchableOpacity onPress={this.onSubmitOK.bind(this)}>
                        <View style={[styles.sendButtonView, {}]}>
                            <Text style={styles.sendButtonText}>
                                确定
                            </Text>
                        </View>
                    </TouchableOpacity>
                </View>
                }
            </View>
        );
    }
}

const styles = StyleSheet.create({
    container: {
        alignItems: 'center',
        height: 66,
    },
    viewStyle:{
        justifyContent: 'center',
        alignItems: 'center',
        backgroundColor: '#F5FCFF',
        flexDirection: 'row',
    },
    textInputStyle: {
        width: 180,
        height: 30,
        borderColor: 'gray',
        borderWidth: 1,
        borderRadius: 5
    },
    sendButtonView: {
        width: 80,
        height: 30,
        borderColor: '55F',
        borderWidth: 1 / PixelRatio.get(),
        borderRadius: 8,
        marginLeft: 5,
        backgroundColor: '#88F',
        alignItems: 'center',
        justifyContent: 'center',
    },
    sendButtonText: {
        fontSize: 20,
        color: 'white',
    },
});
