
Pod::Spec.new do |spec|

  spec.name         = "PureScript"
  spec.version      = "0.1.0"
  spec.summary      = "ScriptEngine module of [PureScript]"
  spec.description  = "ScriptEngine module of [PureScript]"

  spec.homepage     = "https://github.com/loonly/PureScript"
  spec.license      = "MIT"
  spec.author             = { "loonly" => "https://github.com/loonly/PureScript" }
  spec.platform     = :ios, '10.1'
 
  spec.source       = { :git => "https://github.com/loonly/PureScript.git", :tag => "#{spec.version}" }


  # ――― Source Code ―――――――――――――――――――――――――――――――――――――――――――――――――――――――――――――― #
  #
  #  CocoaPods is smart about how it includes source code. For source files
  #  giving a folder will include any swift, h, m, mm, c & cpp files.
  #  For header files it will include any header in the folder.
  #  Not including the public_header_files will make all headers public.
  #

  spec.source_files  = "generated/*.{h,m,c,cpp}","lib/include/**/*.h" 
  #,"ScriptEngine.c","main/*.{h,m,c,cpp}", "custom/*.{h,m,c,cpp}"
  
  #spec.exclude_files = "Classes/Exclude"

  # spec.public_header_files = "Classes/**/*.h"


  # ――― Resources ―――――――――――――――――――――――――――――――――――――――――――――――――――――――――――――――― #
  #
  #  A list of resources included with the Pod. These are copied into the
  #  target bundle with a build phase script. Anything else will be cleaned.
  #  You can preserve files from being cleaned, please don't preserve
  #  non-essential files like tests, examples and documentation.
  #

  spec.resources = "Managed" 
  #"Managed/*.{dll,aotdata,exe}"

  #spec.resource_bundles = {'Managed' => ['Managed/*.{dll,aotdata,exe}']}

  # spec.preserve_paths = "FilesToSave", "MoreFilesToSave"


  # ――― Project Linking ―――――――――――――――――――――――――――――――――――――――――――――――――――――――――― #
  #
  #  Link your library with frameworks, or libraries. Libraries do not include
  #  the lib prefix of their name.
  #

  # spec.framework  = "SomeFramework"
  spec.frameworks = "GSS", "UIKit","Foundation"

  # spec.library   = "iconv"
  spec.libraries = "iconv", "z"
  #,"il2cpp","iPhone-lib"

  spec.static_framework = true

  root_path = File.dirname(__FILE__)
  flag_array = Array.new

  flag_array.push("-L#{root_path}/lib")
  flag_array.push("-L#{root_path}/aot")

  flag_array.push("-lObjC")
  flag_array.push("-force_load #{root_path}/lib/libmono-native-unified.a")
  flag_array.push("-force_load #{root_path}/lib/libScriptEngine.a")

  libdir = Dir::open("lib")
  libdir.each do |f|
    if f.include?(".a")
      fname = f.sub(".a","").sub("lib","")
      flag_array.push(%Q[-l"#{fname}"])
    end
  end

  aotdir = Dir::open("aot")
  aotdir.each do |f|
    if f.include?(".a")
      fname = f.sub(".a","").sub("lib","")
      flag_array.push(%Q[-l"#{fname}"])
    end
  end

  flag_array.push(%Q[-l"iconv"])
  flag_array.push(%Q[-l"z"])

  flag_array.push("$(inherited)")

  spec.vendored_libraries = "lib/*.a", "aot/*.a"
  #spec.header_dir = "lib/include"
  # ――― Project Settings ――――――――――――――――――――――――――――――――――――――――――――――――――――――――― #
  #
  #  If your library depends on compiler flags you can set them in the xcconfig hash
  #  where they will only apply to your library. If you depend on other Podspecs
  #  you can include multiple dependencies to ensure it works.

  # spec.requires_arc = true

  spec.xcconfig = { "ENABLE_BITCODE" => "NO", 
  "HEADER_SEARCH_PATHS" => ["${PODS_TARGET_SRCROOT}/lib/include","${PODS_ROOT}/../Libraries/libil2cpp/include"] ,
  "OTHER_LDFLAGS" => flag_array,
  "GCC_PREPROCESSOR_DEFINITIONS" => "RUNTIME_IOS=1",
  "LIBRARY_SEARCH_PATHS" => ["${PURESCRIPT_DIR}/lib","${PURESCRIPT_DIR}/aot"] #"${PODS_ROOT}/../Libraries"
  }
  # spec.dependency "JSONKit", "~> 1.4"

end
