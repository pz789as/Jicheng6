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
    NativeEventEmitter,
} from 'react-native';

var sms = require('NativeModules').SendSMS;
const smscb = new NativeEventEmitter(sms);

export default class SendSMS extends Component {
    constructor(props) {
        super(props);
        this.sendNumber = '';
        this.state = {
            phoneNumber: '13182922824', //'13675193708',
            blnUpdate: false,
            blnShowVerify: false,
            verifyCode: ''
        };
        this.listener = null;
    }
    setUpdate() {
        this.setState({
            blnUpdate: !this.blnUpdate,
        });
    }
    componentWillMount() {
        this.listener = smscb.addListener('smsCallback', this.smsCallback.bind(this));
    }
    componentDidMount() {
    }
    componentWillUnmount() {
        this.listener && this.listener.remove();
        this.listener = null;
    }
    smsCallback(data){
        if (data.code == 0){
            Alert.alert(
                '提示',
                data.result,
                [
                    { text: "确定", onPress: () => { } }
                ]
            );
        }else if (data.code == 1){
            Alert.alert(
                '提示',
                '发送成功！',
                [
                    { text: "确定", onPress: () => { } }
                ]
            );
        }else if (data.code == 2){
            Alert.alert(
                '提示',
                '验证成功！!',
                [
                    { text: "确定", onPress: () => { } }
                ]
            );
        }
    }
    onTextChange(text) {
        this.setState({
            phoneNumber: text,
        });
    }
    onSubmitSend() {
        var etips = ''
        if (this.state.phoneNumber == '') {
            etips = '手机号为空！';
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
            this.setState({
                blnShowVerify: true,
            });
            sms.sendSMS({
                "PHONE": this.state.phoneNumber,
                "ZONE": '86',
            });
            // this.sendNumber = '';
            // for (var i = 0; i < 6; i++) {
            //     this.sendNumber += parseInt(Math.max(0, Math.random() * 10 - 1)).toString();
            // }
            // console.log(this.sendNumber);
            // var url = 'sms:' +
            //     this.state.phoneNumber +
            //     '&subject=' +
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
    onChangeVerify(text){
        this.setState({
            verifyCode: text,
        });
    }
    onSubmitOK(){
        var etips = ''
        if (this.state.verifyCode == '') {
            etips = '验证码为空！';
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
            sms.sendVerify({
                "VERIFY": this.state.verifyCode,
                "PHONE": this.state.phoneNumber,
                "ZONE": '86',
            });
        }
    }
    render() {
        return (
            <View style={[styles.container, this.props.style ? this.props.style : {}]}>
                <View style={[styles.viewStyle, {marginTop: 0}]}>
                    <TextInput style={styles.textInputStyle}
                        onChangeText={this.onTextChange.bind(this)}
                        value={this.state.phoneNumber}
                        placeholder={'请输入手机号'}/>
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
                            onChangeText={this.onChangeVerify.bind(this)}
                            value={this.state.verifyCode}
                            placeholder={'请输入验证码'}/>
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
    contentStyle:{
        width: 260,
        height: 100,
        borderColor: 'gray',
        borderWidth: 1,
        borderRadius: 5
    },
});
