//
//  SendSMS.m
//  Jicheng6
//
//  Created by guojicheng on 16/11/4.
//  Copyright © 2016年 Facebook. All rights reserved.
//

#import "SendSMS.h"
#import <SMS_SDK/SMSSDK.h>
#import "RCTConvert.h"

@implementation SendSMS

- (instancetype)init
{
  self = [super init];
  if (self) {
    [SMSSDK registerApp:@"18b37710a6538" withSecret:@"776f4cc99b1d0bae4ffc95ab9373a69a"];
  }
  return self;
}

RCT_EXPORT_MODULE();

RCT_EXPORT_METHOD(sendSMS:(NSDictionary*)infos)
{
  [SMSSDK getVerificationCodeByMethod:SMSGetCodeMethodSMS
                          phoneNumber:[RCTConvert NSString:infos[@"PHONE" ]]
                                 zone:[RCTConvert NSString:infos[@"ZONE"]]
                     customIdentifier:nil
                               result:^(NSError *error){
                                 if (!error){
                                   NSLog(@"获取验证码成功");
                                   [self smsCallback:@"1" result:@"获取验证码成功"];
                                 }else{
                                   NSLog(@"错误信息：%@", error);
                                   [self smsCallback:@"0" result:error.description];
                                 }
                               }];
}

RCT_EXPORT_METHOD(sendVerify:(NSDictionary*)infos)
{
  [SMSSDK commitVerificationCode:[RCTConvert NSString:infos[@"VERIFY"]]
                     phoneNumber:[RCTConvert NSString:infos[@"PHONE"]]
                            zone:[RCTConvert NSString:infos[@"ZONE"]]
                          result:^(SMSSDKUserInfo *userInfo, NSError *error){
                            if (!error){
                              NSLog(@"验证成功！");
                              [self smsCallback:@"2" result:userInfo.phone];
                            }else{
                              NSLog(@"错误信息：%@", error);
                              [self smsCallback:@"0" result:error.description];
                            }
                          }];
}

- (NSArray<NSString *> *)supportedEvents
{
  return @[@"smsCallback"];//有几个就写几个
}

-(void)smsCallback:(NSString*)code result:(NSString*) result
{
  [self sendEventWithName:@"smsCallback"
                     body:@{
                            @"code": code,
                            @"result": result,
                            }];
}

@end
