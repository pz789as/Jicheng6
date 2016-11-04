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

export default class SendSMS extends Component {
    constructor(props) {
        super(props);
        this.sendNumber = '';
        this.state = {
            phoneNumber: '13675193708',
            blnUpdate: false
        };
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
            this.sendNumber = '';
            for (var i = 0; i < 6; i++) {
                this.sendNumber += parseInt(Math.max(0, Math.random() * 10 - 1)).toString();
            }
            console.log(this.sendNumber);
            var url = 'sms:' +
                this.state.phoneNumber +
                '&subject=' +
                'Jicheng6短信验证' +
                '&body=' +
                this.sendNumber;
            Linking.canOpenURL(url)
                .then((supported) => {
                    if (!supported) {
                        console.log('Can\'t handle url: ' + url);
                        Alert.alert(
                            '提示',
                            'Can\'t handle url: ' + url,
                            [
                                { text: 'OK', onPress: () => { } }
                            ]
                        );
                    } else {
                        return Linking.openURL(url);
                    }
                })
                .catch((err) => {
                    console.log('An error occurred', err);
                    Alert.alert(
                        '提示',
                        'An error occurred: ' + err,
                        [
                            { text: 'OK', onPress: () => { } }
                        ]
                    );
                });
        }
    }
    render() {
        return (
            <View style={[styles.container, this.props.style ? this.props.style : {}]}>
                <TextInput style={styles.textInputStyle}
                    onChangeText={this.onTextChange.bind(this)}
                    value={this.state.phoneNumber}
                    placeholder={'请输入手机号'}
                    onSubmitEditing={this.onSubmitSend.bind(this)}
                    keyboardType='numeric' />
                <TouchableOpacity onPress={this.onSubmitSend.bind(this)}>
                    <View style={[styles.sendButtonView, {}]}>
                        <Text style={styles.sendButtonText}>
                            发送
                        </Text>
                    </View>
                </TouchableOpacity>
            </View>
        );
    }
}

const styles = StyleSheet.create({
    container: {
        justifyContent: 'center',
        alignItems: 'center',
        backgroundColor: '#F5FCFF',
        flexDirection: 'row'
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
