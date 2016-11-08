//
//  KeyboardHeight.m
//  Jicheng6
//
//  Created by guojicheng on 16/11/7.
//  Copyright © 2016年 Facebook. All rights reserved.
//

#import "KeyboardHeight.h"

@implementation KeyboardHeight

RCT_EXPORT_MODULE();

- (instancetype)init
{
  self = [super init];
  if (self) {
    self.kbHeight = 0;
//    [[NSNotificationCenter defaultCenter] addObserver:self
//                                             selector:@selector(keyboardWillShow:)
//                                                 name:UIKeyboardWillShowNotification
//                                               object:nil];
    [[NSNotificationCenter defaultCenter] addObserver:self
                                             selector:@selector(keyboardDidShow:)
                                                 name:UIKeyboardDidShowNotification
                                               object:nil];
  }
  return self;
}

//-(void)keyboardWillShow:(NSNotification*) aNotification
//{
//  //获取键盘的高度
//  NSDictionary *userInfo = [aNotification userInfo];
//  NSValue *aValue = [userInfo objectForKey:UIKeyboardFrameEndUserInfoKey];
//  CGRect keyboardRect = [aValue CGRectValue];
//  self.kbHeight = keyboardRect.size.height;
//}

-(void)keyboardDidShow:(NSNotification*) aNotification
{
  //获取键盘的高度
  NSDictionary *userInfo = [aNotification userInfo];
  NSValue *aValue = [userInfo objectForKey:UIKeyboardFrameEndUserInfoKey];
  CGRect keyboardRect = [aValue CGRectValue];
  if (_kbHeight != keyboardRect.size.height){
    _kbHeight = keyboardRect.size.height;
    [self heightChanged:_kbHeight];
  }
}

RCT_REMAP_METHOD(getKBHeight,
                 resolver:(RCTPromiseResolveBlock)resolve
                 rejecter:(RCTPromiseRejectBlock)reject)
{
  resolve([[NSNumber alloc]initWithInt:_kbHeight]);
}

- (NSArray<NSString *> *)supportedEvents
{
  return @[@"heightChanged"];
}

-(void)heightChanged:(int)height
{
  [self sendEventWithName:@"heightChanged" body:[NSNumber numberWithUnsignedInt:height]];
}

@end
