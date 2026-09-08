--[[
    Photon UI Library
    A modern, dark-themed, multi-column Roblox UI Library inspired by Photon.
    Universal compatibility: Works in both Roblox Studio (PlayerGui) and Executor environments (CoreGui / gethui).
]]

local TweenService = game:GetService("TweenService")
local UserInputService = game:GetService("UserInputService")
local RunService = game:GetService("RunService")
local CoreGui = game:GetService("CoreGui")
local Players = game:GetService("Players")

local LocalPlayer = Players.LocalPlayer

-- GUI Container resolution
local function getGuiContainer()
    if gethui then
        return gethui()
    elseif syn and syn.protect_gui then
        local folder = Instance.new("Folder")
        pcall(function()
            syn.protect_gui(folder)
            folder.Parent = CoreGui
        end)
        if folder.Parent == CoreGui then
            return folder
        end
    end
    
    local success, _ = pcall(function()
        local test = CoreGui:GetChildren()
    end)
    if success and not RunService:IsStudio() then
        return CoreGui
    end

    if LocalPlayer then
        local playerGui = LocalPlayer:FindFirstChildOfClass("PlayerGui") or LocalPlayer:WaitForChild("PlayerGui", 5)
        if playerGui then
            return playerGui
        end
    end

    return CoreGui
end

local Photon = {
    Themes = {
        Default = {
            Background = Color3.fromRGB(18, 18, 22),
            HeaderBg = Color3.fromRGB(15, 15, 18),
            SidebarBg = Color3.fromRGB(16, 16, 20),
            BottomBarBg = Color3.fromRGB(15, 15, 18),
            CardBg = Color3.fromRGB(24, 24, 28),
            CardHeaderBg = Color3.fromRGB(28, 28, 34),
            ElementBg = Color3.fromRGB(30, 30, 36),
            Border = Color3.fromRGB(42, 42, 50),
            BorderLight = Color3.fromRGB(58, 58, 70),
            Accent = Color3.fromRGB(235, 45, 58),
            AccentDim = Color3.fromRGB(180, 30, 40),
            AccentGlow = Color3.fromRGB(255, 60, 75),
            Text = Color3.fromRGB(230, 230, 235),
            TextDim = Color3.fromRGB(140, 140, 152),
            TextDark = Color3.fromRGB(90, 90, 102),
            Success = Color3.fromRGB(50, 215, 75),
            ToggleInactive = Color3.fromRGB(38, 38, 46)
        }
    }
}

-- Utility helpers
local function tween(object, info, properties)
    local t = TweenService:Create(object, info, properties)
    t:Play()
    return t
end

local function makeDraggable(topbar, mainFrame)
    local dragging = false
    local dragInput, dragStart, startPos

    topbar.InputBegan:Connect(function(input)
        if input.UserInputType == Enum.UserInputType.MouseButton1 or input.UserInputType == Enum.UserInputType.Touch then
            dragging = true
            dragStart = input.Position
            startPos = mainFrame.Position

            input.Changed:Connect(function()
                if input.UserInputState == Enum.UserInputState.End then
                    dragging = false
                end
            end)
        end
    end)

    topbar.InputChanged:Connect(function(input)
        if input.UserInputType == Enum.UserInputType.MouseMovement or input.UserInputType == Enum.UserInputType.Touch then
            dragInput = input
        end
    end)

    UserInputService.InputChanged:Connect(function(input)
        if input == dragInput and dragging then
            local delta = input.Position - dragStart
            mainFrame.Position = UDim2.new(
                startPos.X.Scale,
                startPos.X.Offset + delta.X,
                startPos.Y.Scale,
                startPos.Y.Offset + delta.Y
            )
        end
    end)
end

-- Window Constructor
function Photon:CreateWindow(config)
    config = config or {}
    local windowTitle = config.Title or "photon"
    local windowSize = config.Size or UDim2.new(0, 720, 0, 490)
    local toggleKey = config.Keybind or Enum.KeyCode.RightShift
    local theme = config.Theme or Photon.Themes.Default

    local ScreenGui = Instance.new("ScreenGui")
    ScreenGui.Name = "PhotonUI_" .. tostring(math.random(1000, 9999))
    ScreenGui.ResetOnSpawn = false
    ScreenGui.ZIndexBehavior = Enum.ZIndexBehavior.Sibling
    ScreenGui.Parent = getGuiContainer()

    local Window = {
        Gui = ScreenGui,
        Theme = theme,
        Categories = {},
        ActiveCategory = nil,
        ActiveSubTab = nil,
        Visible = true,
        ActivePopups = {}
    }

    -- Close open popups when clicking elsewhere
    UserInputService.InputBegan:Connect(function(input)
        if input.UserInputType == Enum.UserInputType.MouseButton1 then
            for popup, closeFunc in pairs(Window.ActivePopups) do
                if popup and popup.Parent and popup.Visible then
                    local mousePos = UserInputService:GetMouseLocation()
                    local absPos = popup.AbsolutePosition
                    local absSize = popup.AbsoluteSize
                    if mousePos.X < absPos.X or mousePos.X > absPos.X + absSize.X or
                       mousePos.Y < absPos.Y or mousePos.Y > absPos.Y + absSize.Y then
                        closeFunc()
                    end
                end
            end
        end
    end)

    -- Keybind toggle
    UserInputService.InputBegan:Connect(function(input, processed)
        if not processed and input.KeyCode == toggleKey then
            Window.Visible = not Window.Visible
            ScreenGui.Enabled = Window.Visible
        end
    end)

    -- Main Frame
    local MainFrame = Instance.new("Frame")
    MainFrame.Name = "MainFrame"
    MainFrame.Size = windowSize
    MainFrame.Position = UDim2.new(0.5, -windowSize.X.Offset / 2, 0.5, -windowSize.Y.Offset / 2)
    MainFrame.BackgroundColor3 = theme.Background
    MainFrame.BorderSizePixel = 0
    MainFrame.ClipsDescendants = false
    MainFrame.Parent = ScreenGui

    local MainCorner = Instance.new("UICorner")
    MainCorner.CornerRadius = UDim.new(0, 8)
    MainCorner.Parent = MainFrame

    local MainStroke = Instance.new("UIStroke")
    MainStroke.Color = theme.Border
    MainStroke.Thickness = 1
    MainStroke.Parent = MainFrame

    -- Header / Titlebar
    local Header = Instance.new("Frame")
    Header.Name = "Header"
    Header.Size = UDim2.new(1, 0, 0, 36)
    Header.BackgroundColor3 = theme.HeaderBg
    Header.BorderSizePixel = 0
    Header.Parent = MainFrame

    local HeaderCorner = Instance.new("UICorner")
    HeaderCorner.CornerRadius = UDim.new(0, 8)
    HeaderCorner.Parent = Header

    -- Fix bottom corners of header
    local HeaderHider = Instance.new("Frame")
    HeaderHider.Name = "HeaderHider"
    HeaderHider.Size = UDim2.new(1, 0, 0, 10)
    HeaderHider.Position = UDim2.new(0, 0, 1, -10)
    HeaderHider.BackgroundColor3 = theme.HeaderBg
    HeaderHider.BorderSizePixel = 0
    HeaderHider.Parent = Header

    local HeaderBorder = Instance.new("Frame")
    HeaderBorder.Name = "HeaderBorder"
    HeaderBorder.Size = UDim2.new(1, 0, 0, 1)
    HeaderBorder.Position = UDim2.new(0, 0, 1, 0)
    HeaderBorder.BackgroundColor3 = theme.Border
    HeaderBorder.BorderSizePixel = 0
    HeaderBorder.Parent = Header

    -- Brand Title
    local TitleLabel = Instance.new("TextLabel")
    TitleLabel.Name = "Title"
    TitleLabel.Size = UDim2.new(0, 200, 1, 0)
    TitleLabel.Position = UDim2.new(0, 14, 0, 0)
    TitleLabel.BackgroundTransparency = 1
    TitleLabel.Font = Enum.Font.GothamBold
    TitleLabel.TextSize = 16
    TitleLabel.TextXAlignment = Enum.TextXAlignment.Left
    TitleLabel.TextColor3 = theme.Text
    TitleLabel.Text = windowTitle
    TitleLabel.Parent = Header

    -- Accent dot or line
    local Dot = Instance.new("Frame")
    Dot.Name = "AccentDot"
    Dot.Size = UDim2.new(0, 5, 0, 5)
    Dot.Position = UDim2.new(0, TitleLabel.TextBounds.X + 16, 0.5, -2)
    Dot.BackgroundColor3 = theme.Accent
    Dot.BorderSizePixel = 0
    Dot.Parent = Header
    local DotCorner = Instance.new("UICorner")
    DotCorner.CornerRadius = UDim.new(1, 0)
    DotCorner.Parent = Dot

    -- Window Controls (Minimize, Close)
    local ControlsFrame = Instance.new("Frame")
    ControlsFrame.Name = "Controls"
    ControlsFrame.Size = UDim2.new(0, 60, 1, 0)
    ControlsFrame.Position = UDim2.new(1, -65, 0, 0)
    ControlsFrame.BackgroundTransparency = 1
    ControlsFrame.Parent = Header

    local ControlsLayout = Instance.new("UIListLayout")
    ControlsLayout.FillDirection = Enum.FillDirection.Horizontal
    ControlsLayout.HorizontalAlignment = Enum.HorizontalAlignment.Right
    ControlsLayout.VerticalAlignment = Enum.VerticalAlignment.Center
    ControlsLayout.Padding = UDim.new(0, 8)
    ControlsLayout.Parent = ControlsFrame

    local function createHeaderButton(symbol, callback)
        local btn = Instance.new("TextButton")
        btn.Size = UDim2.new(0, 22, 0, 22)
        btn.BackgroundColor3 = theme.ElementBg
        btn.Text = symbol
        btn.TextColor3 = theme.TextDim
        btn.Font = Enum.Font.GothamMedium
        btn.TextSize = 13
        btn.AutoButtonColor = false
        btn.Parent = ControlsFrame

        local corner = Instance.new("UICorner")
        corner.CornerRadius = UDim.new(0, 4)
        corner.Parent = btn

        local stroke = Instance.new("UIStroke")
        stroke.Color = theme.Border
        stroke.Thickness = 1
        stroke.Parent = btn

        btn.MouseEnter:Connect(function()
            tween(btn, TweenInfo.new(0.15), {BackgroundColor3 = theme.BorderLight, TextColor3 = theme.Text})
        end)
        btn.MouseLeave:Connect(function()
            tween(btn, TweenInfo.new(0.15), {BackgroundColor3 = theme.ElementBg, TextColor3 = theme.TextDim})
        end)
        btn.MouseButton1Click:Connect(callback)
        return btn
    end

    createHeaderButton("-", function()
        Window.Visible = false
        ScreenGui.Enabled = false
    end)

    createHeaderButton("✕", function()
        ScreenGui:Destroy()
    end)

    makeDraggable(Header, MainFrame)

    -- Bottom Category Bar (Dock)
    local BottomBar = Instance.new("Frame")
    BottomBar.Name = "BottomBar"
    BottomBar.Size = UDim2.new(1, 0, 0, 46)
    BottomBar.Position = UDim2.new(0, 0, 1, -46)
    BottomBar.BackgroundColor3 = theme.BottomBarBg
    BottomBar.BorderSizePixel = 0
    BottomBar.Parent = MainFrame

    local BottomBarCorner = Instance.new("UICorner")
    BottomBarCorner.CornerRadius = UDim.new(0, 8)
    BottomBarCorner.Parent = BottomBar

    local BottomHider = Instance.new("Frame")
    BottomHider.Name = "BottomHider"
    BottomHider.Size = UDim2.new(1, 0, 0, 10)
    BottomHider.Position = UDim2.new(0, 0, 0, 0)
    BottomHider.BackgroundColor3 = theme.BottomBarBg
    BottomHider.BorderSizePixel = 0
    BottomHider.Parent = BottomBar

    local BottomBorder = Instance.new("Frame")
    BottomBorder.Name = "BottomBorder"
    BottomBorder.Size = UDim2.new(1, 0, 0, 1)
    BottomBorder.Position = UDim2.new(0, 0, 0, 0)
    BottomBorder.BackgroundColor3 = theme.Border
    BottomBorder.BorderSizePixel = 0
    BottomBorder.Parent = BottomBar

    local BottomLayout = Instance.new("UIListLayout")
    BottomLayout.FillDirection = Enum.FillDirection.Horizontal
    BottomLayout.HorizontalAlignment = Enum.HorizontalAlignment.Center
    BottomLayout.VerticalAlignment = Enum.VerticalAlignment.Center
    BottomLayout.Padding = UDim.new(0, 6)
    BottomLayout.Parent = BottomBar

    -- Content Area (Between Header and BottomBar)
    local ContentArea = Instance.new("Frame")
    ContentArea.Name = "ContentArea"
    ContentArea.Size = UDim2.new(1, 0, 1, -82)
    ContentArea.Position = UDim2.new(0, 0, 0, 36)
    ContentArea.BackgroundTransparency = 1
    ContentArea.Parent = MainFrame

    -- Left Sidebar for Sub-Tabs
    local Sidebar = Instance.new("Frame")
    Sidebar.Name = "Sidebar"
    Sidebar.Size = UDim2.new(0, 110, 1, 0)
    Sidebar.BackgroundColor3 = theme.SidebarBg
    Sidebar.BorderSizePixel = 0
    Sidebar.Parent = ContentArea

    local SidebarBorder = Instance.new("Frame")
    SidebarBorder.Name = "SidebarBorder"
    SidebarBorder.Size = UDim2.new(0, 1, 1, 0)
    SidebarBorder.Position = UDim2.new(1, -1, 0, 0)
    SidebarBorder.BackgroundColor3 = theme.Border
    SidebarBorder.BorderSizePixel = 0
    SidebarBorder.Parent = Sidebar

    local SidebarList = Instance.new("UIListLayout")
    SidebarList.FillDirection = Enum.FillDirection.Vertical
    SidebarList.HorizontalAlignment = Enum.HorizontalAlignment.Center
    SidebarList.VerticalAlignment = Enum.VerticalAlignment.Top
    SidebarList.Padding = UDim.new(0, 4)
    SidebarList.Parent = Sidebar

    local SidebarPadding = Instance.new("UIPadding")
    SidebarPadding.PaddingTop = UDim.new(0, 12)
    SidebarPadding.PaddingLeft = UDim.new(0, 8)
    SidebarPadding.PaddingRight = UDim.new(0, 8)
    SidebarPadding.Parent = Sidebar

    -- Active View Container (Holds cards)
    local PageContainer = Instance.new("Frame")
    PageContainer.Name = "PageContainer"
    PageContainer.Size = UDim2.new(1, -110, 1, 0)
    PageContainer.Position = UDim2.new(0, 110, 0, 0)
    PageContainer.BackgroundTransparency = 1
    PageContainer.ClipsDescendants = true
    PageContainer.Parent = ContentArea

    -- Toast Notification Holder
    local NotificationHolder = Instance.new("Frame")
    NotificationHolder.Name = "NotificationHolder"
    NotificationHolder.Size = UDim2.new(0, 260, 1, -20)
    NotificationHolder.Position = UDim2.new(1, -270, 0, 10)
    NotificationHolder.BackgroundTransparency = 1
    NotificationHolder.ZIndex = 100
    NotificationHolder.Parent = ScreenGui

    local NotifLayout = Instance.new("UIListLayout")
    NotifLayout.VerticalAlignment = Enum.VerticalAlignment.Bottom
    NotifLayout.HorizontalAlignment = Enum.HorizontalAlignment.Right
    NotifLayout.Padding = UDim.new(0, 8)
    NotifLayout.Parent = NotificationHolder

    function Window:Notify(notifyConfig)
        notifyConfig = notifyConfig or {}
        local title = notifyConfig.Title or "Notification"
        local message = notifyConfig.Content or ""
        local duration = notifyConfig.Duration or 3

        local notif = Instance.new("Frame")
        notif.Name = "Notification"
        notif.Size = UDim2.new(1, 0, 0, 0)
        notif.BackgroundColor3 = theme.CardBg
        notif.ClipsDescendants = true
        notif.Parent = NotificationHolder

        local corner = Instance.new("UICorner")
        corner.CornerRadius = UDim.new(0, 6)
        corner.Parent = notif

        local stroke = Instance.new("UIStroke")
        stroke.Color = theme.Border
        stroke.Thickness = 1
        stroke.Parent = notif

        local bar = Instance.new("Frame")
        bar.Name = "AccentBar"
        bar.Size = UDim2.new(0, 3, 1, 0)
        bar.BackgroundColor3 = theme.Accent
        bar.BorderSizePixel = 0
        bar.Parent = notif

        local contentFrame = Instance.new("Frame")
        contentFrame.Size = UDim2.new(1, -12, 1, 0)
        contentFrame.Position = UDim2.new(0, 10, 0, 0)
        contentFrame.BackgroundTransparency = 1
        contentFrame.Parent = notif

        local tLabel = Instance.new("TextLabel")
        tLabel.Size = UDim2.new(1, 0, 0, 20)
        tLabel.Position = UDim2.new(0, 0, 0, 6)
        tLabel.BackgroundTransparency = 1
        tLabel.Font = Enum.Font.GothamBold
        tLabel.TextSize = 13
        tLabel.TextColor3 = theme.Text
        tLabel.TextXAlignment = Enum.TextXAlignment.Left
        tLabel.Text = title
        tLabel.Parent = contentFrame

        local mLabel = Instance.new("TextLabel")
        mLabel.Size = UDim2.new(1, 0, 0, 24)
        mLabel.Position = UDim2.new(0, 0, 0, 24)
        mLabel.BackgroundTransparency = 1
        mLabel.Font = Enum.Font.Gotham
        mLabel.TextSize = 12
        mLabel.TextColor3 = theme.TextDim
        mLabel.TextXAlignment = Enum.TextXAlignment.Left
        mLabel.TextWrapped = true
        mLabel.Text = message
        mLabel.Parent = contentFrame

        tween(notif, TweenInfo.new(0.25, Enum.EasingStyle.Quart, Enum.EasingDirection.Out), {Size = UDim2.new(1, 0, 0, 56)})

        task.delay(duration, function()
            if notif and notif.Parent then
                local closeTween = tween(notif, TweenInfo.new(0.2, Enum.EasingStyle.Quart, Enum.EasingDirection.In), {
                    Size = UDim2.new(1, 0, 0, 0),
                    BackgroundTransparency = 1
                })
                closeTween.Completed:Connect(function()
                    notif:Destroy()
                end)
            end
        end)
    end

    -- Add Category Method
    function Window:AddCategory(catConfig)
        catConfig = catConfig or {}
        local catName = catConfig.Title or "Category"
        local catIcon = catConfig.Icon or ""

        local Category = {
            Title = catName,
            SubTabs = {},
            ActiveSubTab = nil,
            SubTabButtons = {}
        }

        -- Category Bottom Button
        local CatButton = Instance.new("TextButton")
        CatButton.Name = "CatBtn_" .. catName
        CatButton.Size = UDim2.new(0, 92, 0, 36)
        CatButton.BackgroundColor3 = theme.BottomBarBg
        CatButton.BackgroundTransparency = 1
        CatButton.Text = ""
        CatButton.AutoButtonColor = false
        CatButton.Parent = BottomBar

        local CatCorner = Instance.new("UICorner")
        CatCorner.CornerRadius = UDim.new(0, 6)
        CatCorner.Parent = CatButton

        local CatContent = Instance.new("Frame")
        CatContent.Size = UDim2.new(1, 0, 1, 0)
        CatContent.BackgroundTransparency = 1
        CatContent.Parent = CatButton

        local CatLayout = Instance.new("UIListLayout")
        CatLayout.FillDirection = Enum.FillDirection.Vertical
        CatLayout.HorizontalAlignment = Enum.HorizontalAlignment.Center
        CatLayout.VerticalAlignment = Enum.VerticalAlignment.Center
        CatLayout.Padding = UDim.new(0, 2)
        CatLayout.Parent = CatContent

        -- Icon (if provided)
        local IconImage = nil
        if catIcon and catIcon ~= "" then
            IconImage = Instance.new("ImageLabel")
            IconImage.Size = UDim2.new(0, 16, 0, 16)
            IconImage.BackgroundTransparency = 1
            IconImage.Image = catIcon
            IconImage.ImageColor3 = theme.TextDim
            IconImage.Parent = CatContent
        end

        local CatText = Instance.new("TextLabel")
        CatText.Size = UDim2.new(1, 0, 0, 14)
        CatText.BackgroundTransparency = 1
        CatText.Font = Enum.Font.GothamMedium
        CatText.TextSize = 12
        CatText.TextColor3 = theme.TextDim
        CatText.Text = catName
        CatText.Parent = CatContent

        -- Active indicator bar
        local ActivePill = Instance.new("Frame")
        ActivePill.Name = "ActivePill"
        ActivePill.Size = UDim2.new(0, 24, 0, 2)
        ActivePill.Position = UDim2.new(0.5, -12, 1, -2)
        ActivePill.BackgroundColor3 = theme.Accent
        ActivePill.BackgroundTransparency = 1
        ActivePill.BorderSizePixel = 0
        ActivePill.Parent = CatButton

        local PillCorner = Instance.new("UICorner")
        PillCorner.CornerRadius = UDim.new(1, 0)
        PillCorner.Parent = ActivePill

        Category.Button = CatButton
        Category.TextLabel = CatText
        Category.IconImage = IconImage
        Category.ActivePill = ActivePill

        function Category:Select()
            for _, c in pairs(Window.Categories) do
                c:Deselect()
            end
            Window.ActiveCategory = Category
            tween(CatText, TweenInfo.new(0.2), {TextColor3 = theme.Text})
            if IconImage then
                tween(IconImage, TweenInfo.new(0.2), {ImageColor3 = theme.Accent})
            end
            tween(ActivePill, TweenInfo.new(0.2), {BackgroundTransparency = 0})

            -- Show sub-tabs in sidebar
            for _, b in pairs(Sidebar:GetChildren()) do
                if b:IsA("TextButton") then
                    b.Visible = false
                end
            end
            for _, btn in pairs(Category.SubTabButtons) do
                btn.Visible = true
            end

            -- Select first subtab if available
            if Category.ActiveSubTab then
                Category.ActiveSubTab:Select()
            elseif #Category.SubTabs > 0 then
                Category.SubTabs[1]:Select()
            end
        end

        function Category:Deselect()
            tween(CatText, TweenInfo.new(0.2), {TextColor3 = theme.TextDim})
            if IconImage then
                tween(IconImage, TweenInfo.new(0.2), {ImageColor3 = theme.TextDim})
            end
            tween(ActivePill, TweenInfo.new(0.2), {BackgroundTransparency = 1})
            if Category.ActiveSubTab then
                Category.ActiveSubTab:Deselect()
            end
        end

        CatButton.MouseButton1Click:Connect(function()
            Category:Select()
        end)

        -- Sub-Tab Constructor
        function Category:AddSubTab(subConfig)
            subConfig = subConfig or {}
            local subTitle = subConfig.Title or "Tab"

            local SubTab = {
                Title = subTitle,
                Columns = {},
                PageFrame = nil
            }

            -- Sidebar Button
            local SubBtn = Instance.new("TextButton")
            SubBtn.Name = "SubBtn_" .. subTitle
            SubBtn.Size = UDim2.new(1, 0, 0, 26)
            SubBtn.BackgroundColor3 = theme.ElementBg
            SubBtn.BackgroundTransparency = 1
            SubBtn.Text = subTitle
            SubBtn.Font = Enum.Font.GothamMedium
            SubBtn.TextSize = 13
            SubBtn.TextColor3 = theme.TextDark
            SubBtn.TextXAlignment = Enum.TextXAlignment.Left
            SubBtn.AutoButtonColor = false
            SubBtn.Visible = false
            SubBtn.Parent = Sidebar

            local SubBtnPad = Instance.new("UIPadding")
            SubBtnPad.PaddingLeft = UDim.new(0, 8)
            SubBtnPad.Parent = SubBtn

            local SubBtnCorner = Instance.new("UICorner")
            SubBtnCorner.CornerRadius = UDim.new(0, 4)
            SubBtnCorner.Parent = SubBtn

            -- Page Container for this SubTab
            local PageFrame = Instance.new("ScrollingFrame")
            PageFrame.Name = "Page_" .. subTitle
            PageFrame.Size = UDim2.new(1, 0, 1, 0)
            PageFrame.BackgroundTransparency = 1
            PageFrame.BorderSizePixel = 0
            PageFrame.ScrollBarThickness = 3
            PageFrame.ScrollBarImageColor3 = theme.BorderLight
            PageFrame.CanvasSize = UDim2.new(0, 0, 0, 0)
            PageFrame.AutomaticCanvasSize = Enum.AutomaticSize.Y
            PageFrame.Visible = false
            PageFrame.Parent = PageContainer

            local PageLayout = Instance.new("UIListLayout")
            PageLayout.FillDirection = Enum.FillDirection.Horizontal
            PageLayout.HorizontalAlignment = Enum.HorizontalAlignment.Left
            PageLayout.VerticalAlignment = Enum.VerticalAlignment.Top
            PageLayout.Padding = UDim.new(0, 10)
            PageLayout.Parent = PageFrame

            local PagePad = Instance.new("UIPadding")
            PagePad.PaddingTop = UDim.new(0, 12)
            PagePad.PaddingBottom = UDim.new(0, 12)
            PagePad.PaddingLeft = UDim.new(0, 12)
            PagePad.PaddingRight = UDim.new(0, 12)
            PagePad.Parent = PageFrame

            SubTab.Button = SubBtn
            SubTab.PageFrame = PageFrame

            function SubTab:Select()
                for _, s in pairs(Category.SubTabs) do
                    s:Deselect()
                end
                Category.ActiveSubTab = SubTab
                Window.ActiveSubTab = SubTab
                tween(SubBtn, TweenInfo.new(0.2), {
                    TextColor3 = theme.Text,
                    BackgroundTransparency = 0.8
                })
                PageFrame.Visible = true
            end

            function SubTab:Deselect()
                tween(SubBtn, TweenInfo.new(0.2), {
                    TextColor3 = theme.TextDark,
                    BackgroundTransparency = 1
                })
                PageFrame.Visible = false
            end

            SubBtn.MouseButton1Click:Connect(function()
                SubTab:Select()
            end)

            table.insert(Category.SubTabButtons, SubBtn)
            table.insert(Category.SubTabs, SubTab)

            -- Column Constructor
            function SubTab:AddColumn(width)
                local colWidth = width or 185
                local Column = {}

                local ColFrame = Instance.new("Frame")
                ColFrame.Name = "Column"
                ColFrame.Size = UDim2.new(0, colWidth, 0, 0)
                ColFrame.AutomaticSize = Enum.AutomaticSize.Y
                ColFrame.BackgroundTransparency = 1
                ColFrame.Parent = PageFrame

                local ColLayout = Instance.new("UIListLayout")
                ColLayout.FillDirection = Enum.FillDirection.Vertical
                ColLayout.HorizontalAlignment = Enum.HorizontalAlignment.Center
                ColLayout.VerticalAlignment = Enum.VerticalAlignment.Top
                ColLayout.Padding = UDim.new(0, 10)
                ColLayout.Parent = ColFrame

                Column.Frame = ColFrame

                -- Card / Groupbox Constructor
                function Column:AddCard(cardConfig)
                    cardConfig = cardConfig or {}
                    local cardTitle = cardConfig.Title or "Section"

                    local Card = {}

                    local CardFrame = Instance.new("Frame")
                    CardFrame.Name = "Card_" .. cardTitle
                    CardFrame.Size = UDim2.new(1, 0, 0, 0)
                    CardFrame.AutomaticSize = Enum.AutomaticSize.Y
                    CardFrame.BackgroundColor3 = theme.CardBg
                    CardFrame.BorderSizePixel = 0
                    CardFrame.Parent = ColFrame

                    local CardCorner = Instance.new("UICorner")
                    CardCorner.CornerRadius = UDim.new(0, 6)
                    CardCorner.Parent = CardFrame

                    local CardStroke = Instance.new("UIStroke")
                    CardStroke.Color = theme.Border
                    CardStroke.Thickness = 1
                    CardStroke.Parent = CardFrame

                    -- Card Header
                    local CardHeader = Instance.new("Frame")
                    CardHeader.Name = "CardHeader"
                    CardHeader.Size = UDim2.new(1, 0, 0, 28)
                    CardHeader.BackgroundTransparency = 1
                    CardHeader.Parent = CardFrame

                    local TitleText = Instance.new("TextLabel")
                    TitleText.Size = UDim2.new(1, -16, 1, 0)
                    TitleText.Position = UDim2.new(0, 10, 0, 0)
                    TitleText.BackgroundTransparency = 1
                    TitleText.Font = Enum.Font.GothamBold
                    TitleText.TextSize = 13
                    TitleText.TextColor3 = theme.Text
                    TitleText.TextXAlignment = Enum.TextXAlignment.Left
                    TitleText.Text = cardTitle
                    TitleText.Parent = CardHeader

                    local CardHeaderLine = Instance.new("Frame")
                    CardHeaderLine.Name = "Line"
                    CardHeaderLine.Size = UDim2.new(1, 0, 0, 1)
                    CardHeaderLine.Position = UDim2.new(0, 0, 1, 0)
                    CardHeaderLine.BackgroundColor3 = theme.Border
                    CardHeaderLine.BorderSizePixel = 0
                    CardHeaderLine.Parent = CardHeader

                    -- Elements Container
                    local ElementsContainer = Instance.new("Frame")
                    ElementsContainer.Name = "Elements"
                    ElementsContainer.Size = UDim2.new(1, 0, 0, 0)
                    ElementsContainer.Position = UDim2.new(0, 0, 0, 29)
                    ElementsContainer.AutomaticSize = Enum.AutomaticSize.Y
                    ElementsContainer.BackgroundTransparency = 1
                    ElementsContainer.Parent = CardFrame

                    local ELayout = Instance.new("UIListLayout")
                    ELayout.FillDirection = Enum.FillDirection.Vertical
                    ELayout.HorizontalAlignment = Enum.HorizontalAlignment.Center
                    ELayout.VerticalAlignment = Enum.VerticalAlignment.Top
                    ELayout.Padding = UDim.new(0, 7)
                    ELayout.Parent = ElementsContainer

                    local EPadding = Instance.new("UIPadding")
                    EPadding.PaddingTop = UDim.new(0, 8)
                    EPadding.PaddingBottom = UDim.new(0, 10)
                    EPadding.PaddingLeft = UDim.new(0, 10)
                    EPadding.PaddingRight = UDim.new(0, 10)
                    EPadding.Parent = ElementsContainer

                    Card.Frame = CardFrame
                    Card.Container = ElementsContainer

                    -- Sub-Tabs inside a Card (e.g. "Radar" | "Aimbot" segment switcher)
                    function Card:AddSegmentedTab(tabs, onTabSelected)
                        tabs = tabs or {}
                        local currentActive = tabs[1] or ""

                        local SegContainer = Instance.new("Frame")
                        SegContainer.Name = "SegmentedControl"
                        SegContainer.Size = UDim2.new(1, 0, 0, 24)
                        SegContainer.BackgroundColor3 = theme.ElementBg
                        SegContainer.Parent = ElementsContainer

                        local SegCorner = Instance.new("UICorner")
                        SegCorner.CornerRadius = UDim.new(0, 4)
                        SegCorner.Parent = SegContainer

                        local SegStroke = Instance.new("UIStroke")
                        SegStroke.Color = theme.Border
                        SegStroke.Thickness = 1
                        SegStroke.Parent = SegContainer

                        local SegLayout = Instance.new("UIListLayout")
                        SegLayout.FillDirection = Enum.FillDirection.Horizontal
                        SegLayout.HorizontalAlignment = Enum.HorizontalAlignment.Center
                        SegLayout.VerticalAlignment = Enum.VerticalAlignment.Center
                        SegLayout.Parent = SegContainer

                        local tabButtons = {}
                        local tabCount = #tabs
                        local tabWidthScale = 1 / math.max(1, tabCount)

                        for _, tabName in ipairs(tabs) do
                            local tBtn = Instance.new("TextButton")
                            tBtn.Name = "Tab_" .. tabName
                            tBtn.Size = UDim2.new(tabWidthScale, 0, 1, 0)
                            tBtn.BackgroundTransparency = 1
                            tBtn.Text = tabName
                            tBtn.Font = Enum.Font.GothamMedium
                            tBtn.TextSize = 12
                            tBtn.TextColor3 = (tabName == currentActive) and theme.Accent or theme.TextDim
                            tBtn.AutoButtonColor = false
                            tBtn.Parent = SegContainer

                            table.insert(tabButtons, {Name = tabName, Button = tBtn})

                            tBtn.MouseButton1Click:Connect(function()
                                currentActive = tabName
                                for _, item in ipairs(tabButtons) do
                                    if item.Name == tabName then
                                        tween(item.Button, TweenInfo.new(0.15), {TextColor3 = theme.Accent})
                                    else
                                        tween(item.Button, TweenInfo.new(0.15), {TextColor3 = theme.TextDim})
                                    end
                                end
                                if onTabSelected then
                                    onTabSelected(tabName)
                                end
                            end)
                        end
                    end

                    -- Helper: Floating Context / Sub-Settings Menu (as seen on "Chams" gear click)
                    local function createFloatingContextMenu(triggerButton, title)
                        local Popup = Instance.new("Frame")
                        Popup.Name = "ContextMenu_" .. title
                        Popup.Size = UDim2.new(0, 190, 0, 0)
                        Popup.AutomaticSize = Enum.AutomaticSize.Y
                        Popup.BackgroundColor3 = theme.CardBg
                        Popup.BorderSizePixel = 0
                        Popup.ZIndex = 50
                        Popup.Visible = false
                        Popup.Parent = ScreenGui

                        local pCorner = Instance.new("UICorner")
                        pCorner.CornerRadius = UDim.new(0, 6)
                        pCorner.Parent = Popup

                        local pStroke = Instance.new("UIStroke")
                        pStroke.Color = theme.BorderLight
                        pStroke.Thickness = 1
                        pStroke.Parent = Popup

                        local pLayout = Instance.new("UIListLayout")
                        pLayout.FillDirection = Enum.FillDirection.Vertical
                        pLayout.HorizontalAlignment = Enum.HorizontalAlignment.Center
                        pLayout.VerticalAlignment = Enum.VerticalAlignment.Top
                        pLayout.Padding = UDim.new(0, 6)
                        pLayout.Parent = Popup

                        local pPad = Instance.new("UIPadding")
                        pPad.PaddingTop = UDim.new(0, 8)
                        pPad.PaddingBottom = UDim.new(0, 8)
                        pPad.PaddingLeft = UDim.new(0, 8)
                        pPad.PaddingRight = UDim.new(0, 8)
                        pPad.Parent = Popup

                        local function updatePosition()
                            local triggerPos = triggerButton.AbsolutePosition
                            local triggerSize = triggerButton.AbsoluteSize
                            Popup.Position = UDim2.new(0, triggerPos.X + triggerSize.X + 8, 0, triggerPos.Y - 10)
                        end

                        local MenuApi = {
                            Frame = Popup,
                            Visible = false
                        }

                        function MenuApi:Toggle()
                            MenuApi.Visible = not MenuApi.Visible
                            if MenuApi.Visible then
                                updatePosition()
                                Popup.Visible = true
                                Window.ActivePopups[Popup] = function()
                                    MenuApi:Close()
                                end
                            else
                                MenuApi:Close()
                            end
                        end

                        function MenuApi:Close()
                            MenuApi.Visible = false
                            Popup.Visible = false
                            Window.ActivePopups[Popup] = nil
                        end

                        -- Add Checkbox inside context menu
                        function MenuApi:AddToggle(optConfig)
                            optConfig = optConfig or {}
                            local optText = optConfig.Title or "Option"
                            local optState = optConfig.Default or false
                            local callback = optConfig.Callback or function() end

                            local row = Instance.new("TextButton")
                            row.Size = UDim2.new(1, 0, 0, 20)
                            row.BackgroundTransparency = 1
                            row.Text = ""
                            row.Parent = Popup

                            local box = Instance.new("Frame")
                            box.Size = UDim2.new(0, 14, 0, 14)
                            box.Position = UDim2.new(0, 0, 0.5, -7)
                            box.BackgroundColor3 = optState and theme.Accent or theme.ToggleInactive
                            box.BorderSizePixel = 0
                            box.Parent = row

                            local bCorner = Instance.new("UICorner")
                            bCorner.CornerRadius = UDim.new(0, 3)
                            bCorner.Parent = box

                            local checkIcon = Instance.new("TextLabel")
                            checkIcon.Size = UDim2.new(1, 0, 1, 0)
                            checkIcon.BackgroundTransparency = 1
                            checkIcon.Text = "✓"
                            checkIcon.TextColor3 = Color3.fromRGB(255, 255, 255)
                            checkIcon.Font = Enum.Font.GothamBold
                            checkIcon.TextSize = 10
                            checkIcon.Visible = optState
                            checkIcon.Parent = box

                            local label = Instance.new("TextLabel")
                            label.Size = UDim2.new(1, -22, 1, 0)
                            label.Position = UDim2.new(0, 20, 0, 0)
                            label.BackgroundTransparency = 1
                            label.Font = Enum.Font.GothamMedium
                            label.TextSize = 11
                            label.TextColor3 = optState and theme.Text or theme.TextDim
                            label.TextXAlignment = Enum.TextXAlignment.Left
                            label.Text = optText
                            label.Parent = row

                            row.MouseButton1Click:Connect(function()
                                optState = not optState
                                checkIcon.Visible = optState
                                tween(box, TweenInfo.new(0.15), {
                                    BackgroundColor3 = optState and theme.Accent or theme.ToggleInactive
                                })
                                tween(label, TweenInfo.new(0.15), {
                                    TextColor3 = optState and theme.Text or theme.TextDim
                                })
                                callback(optState)
                            end)
                            return row
                        end

                        -- Add Dropdown inside context menu
                        function MenuApi:AddDropdown(dConfig)
                            dConfig = dConfig or {}
                            local dTitle = dConfig.Title or "Dropdown"
                            local dOptions = dConfig.Options or {}
                            local dCurrent = dConfig.Default or dOptions[1] or ""
                            local dCallback = dConfig.Callback or function() end

                            local row = Instance.new("Frame")
                            row.Size = UDim2.new(1, 0, 0, 38)
                            row.BackgroundTransparency = 1
                            row.Parent = Popup

                            local label = Instance.new("TextLabel")
                            label.Size = UDim2.new(1, 0, 0, 14)
                            label.BackgroundTransparency = 1
                            label.Font = Enum.Font.GothamMedium
                            label.TextSize = 11
                            label.TextColor3 = theme.TextDim
                            label.TextXAlignment = Enum.TextXAlignment.Left
                            label.Text = dTitle
                            label.Parent = row

                            local btn = Instance.new("TextButton")
                            btn.Size = UDim2.new(1, 0, 0, 20)
                            btn.Position = UDim2.new(0, 0, 0, 16)
                            btn.BackgroundColor3 = theme.ElementBg
                            btn.Text = dCurrent
                            btn.Font = Enum.Font.Gotham
                            btn.TextSize = 10
                            btn.TextColor3 = theme.Text
                            btn.Parent = row

                            local bCorner = Instance.new("UICorner")
                            bCorner.CornerRadius = UDim.new(0, 3)
                            bCorner.Parent = btn

                            local bStroke = Instance.new("UIStroke")
                            bStroke.Color = theme.Border
                            bStroke.Thickness = 1
                            bStroke.Parent = btn

                            local dropList = Instance.new("Frame")
                            dropList.Size = UDim2.new(1, 0, 0, 0)
                            dropList.Position = UDim2.new(0, 0, 1, 2)
                            dropList.BackgroundColor3 = theme.ElementBg
                            dropList.BorderSizePixel = 0
                            dropList.ZIndex = 60
                            dropList.Visible = false
                            dropList.Parent = btn

                            local dlCorner = Instance.new("UICorner")
                            dlCorner.CornerRadius = UDim.new(0, 3)
                            dlCorner.Parent = dropList

                            local dlLayout = Instance.new("UIListLayout")
                            dlLayout.FillDirection = Enum.FillDirection.Vertical
                            dlLayout.Parent = dropList

                            for _, opt in ipairs(dOptions) do
                                local optBtn = Instance.new("TextButton")
                                optBtn.Size = UDim2.new(1, 0, 0, 18)
                                optBtn.BackgroundTransparency = 1
                                optBtn.Text = opt
                                optBtn.Font = Enum.Font.Gotham
                                optBtn.TextSize = 10
                                optBtn.TextColor3 = (opt == dCurrent) and theme.Accent or theme.TextDim
                                optBtn.ZIndex = 61
                                optBtn.Parent = dropList

                                optBtn.MouseButton1Click:Connect(function()
                                    dCurrent = opt
                                    btn.Text = opt
                                    dropList.Visible = false
                                    dCallback(opt)
                                end)
                            end

                            btn.MouseButton1Click:Connect(function()
                                dropList.Visible = not dropList.Visible
                                dropList.Size = UDim2.new(1, 0, 0, #dOptions * 18)
                            end)
                        end

                        return MenuApi
                    end

                    -- Component: Toggle (with attachments: Keybind, Colorpicker, Gear menu)
                    function Card:AddToggle(toggleConfig)
                        toggleConfig = toggleConfig or {}
                        local toggleTitle = toggleConfig.Title or "Toggle"
                        local state = toggleConfig.Default or false
                        local callback = toggleConfig.Callback or function() end

                        local Row = Instance.new("Frame")
                        Row.Name = "ToggleRow_" .. toggleTitle
                        Row.Size = UDim2.new(1, 0, 0, 20)
                        Row.BackgroundTransparency = 1
                        Row.Parent = ElementsContainer

                        local ClickArea = Instance.new("TextButton")
                        ClickArea.Name = "ClickArea"
                        ClickArea.Size = UDim2.new(1, -70, 1, 0)
                        ClickArea.BackgroundTransparency = 1
                        ClickArea.Text = ""
                        ClickArea.Parent = Row

                        -- Box
                        local Box = Instance.new("Frame")
                        Box.Name = "Checkbox"
                        Box.Size = UDim2.new(0, 14, 0, 14)
                        Box.Position = UDim2.new(0, 0, 0.5, -7)
                        Box.BackgroundColor3 = state and theme.Accent or theme.ToggleInactive
                        Box.BorderSizePixel = 0
                        Box.Parent = ClickArea

                        local BoxCorner = Instance.new("UICorner")
                        BoxCorner.CornerRadius = UDim.new(0, 3)
                        BoxCorner.Parent = Box

                        local Checkmark = Instance.new("TextLabel")
                        Checkmark.Name = "Checkmark"
                        Checkmark.Size = UDim2.new(1, 0, 1, 0)
                        Checkmark.BackgroundTransparency = 1
                        Checkmark.Font = Enum.Font.GothamBold
                        Checkmark.TextSize = 10
                        Checkmark.TextColor3 = Color3.fromRGB(255, 255, 255)
                        Checkmark.Text = "✓"
                        Checkmark.Visible = state
                        Checkmark.Parent = Box

                        local Label = Instance.new("TextLabel")
                        Label.Name = "Label"
                        Label.Size = UDim2.new(1, -22, 1, 0)
                        Label.Position = UDim2.new(0, 22, 0, 0)
                        Label.BackgroundTransparency = 1
                        Label.Font = Enum.Font.GothamMedium
                        Label.TextSize = 12
                        Label.TextColor3 = state and theme.Text or theme.TextDim
                        Label.TextXAlignment = Enum.TextXAlignment.Left
                        Label.Text = toggleTitle
                        Label.Parent = ClickArea

                        local Attachments = Instance.new("Frame")
                        Attachments.Name = "Attachments"
                        Attachments.Size = UDim2.new(0, 70, 1, 0)
                        Attachments.Position = UDim2.new(1, -70, 0, 0)
                        Attachments.BackgroundTransparency = 1
                        Attachments.Parent = Row

                        local AttLayout = Instance.new("UIListLayout")
                        AttLayout.FillDirection = Enum.FillDirection.Horizontal
                        AttLayout.HorizontalAlignment = Enum.HorizontalAlignment.Right
                        AttLayout.VerticalAlignment = Enum.VerticalAlignment.Center
                        AttLayout.Padding = UDim.new(0, 4)
                        AttLayout.Parent = Attachments

                        local ToggleObj = {
                            State = state,
                            Row = Row,
                            Attachments = Attachments
                        }

                        local function updateState(newState)
                            state = newState
                            ToggleObj.State = state
                            Checkmark.Visible = state
                            tween(Box, TweenInfo.new(0.15), {
                                BackgroundColor3 = state and theme.Accent or theme.ToggleInactive
                            })
                            tween(Label, TweenInfo.new(0.15), {
                                TextColor3 = state and theme.Text or theme.TextDim
                            })
                            callback(state)
                        end

                        ClickArea.MouseButton1Click:Connect(function()
                            updateState(not state)
                        end)

                        function ToggleObj:Set(newState)
                            updateState(newState)
                        end

                        -- Attachment: Inline Keybind (e.g. "Bind")
                        function ToggleObj:AddKeybind(bindConfig)
                            bindConfig = bindConfig or {}
                            local currentKey = bindConfig.Default or Enum.KeyCode.Unknown
                            local keyCallback = bindConfig.Callback or function() end
                            local listening = false

                            local BindBtn = Instance.new("TextButton")
                            BindBtn.Name = "Keybind"
                            BindBtn.Size = UDim2.new(0, 28, 0, 16)
                            BindBtn.BackgroundColor3 = theme.ElementBg
                            BindBtn.Text = currentKey == Enum.KeyCode.Unknown and "Bind" or currentKey.Name
                            BindBtn.Font = Enum.Font.Gotham
                            BindBtn.TextSize = 10
                            BindBtn.TextColor3 = theme.TextDim
                            BindBtn.AutoButtonColor = false
                            BindBtn.Parent = Attachments

                            local bCorner = Instance.new("UICorner")
                            bCorner.CornerRadius = UDim.new(0, 3)
                            bCorner.Parent = BindBtn

                            local bStroke = Instance.new("UIStroke")
                            bStroke.Color = theme.Border
                            bStroke.Thickness = 1
                            bStroke.Parent = BindBtn

                            BindBtn.MouseButton1Click:Connect(function()
                                listening = true
                                BindBtn.Text = "..."
                                tween(bStroke, TweenInfo.new(0.15), {Color = theme.Accent})
                            end)

                            UserInputService.InputBegan:Connect(function(input, processed)
                                if listening and not processed then
                                    if input.UserInputType == Enum.UserInputType.Keyboard then
                                        listening = false
                                        tween(bStroke, TweenInfo.new(0.15), {Color = theme.Border})
                                        if input.KeyCode == Enum.KeyCode.Escape then
                                            currentKey = Enum.KeyCode.Unknown
                                            BindBtn.Text = "Bind"
                                        else
                                            currentKey = input.KeyCode
                                            BindBtn.Text = currentKey.Name
                                        end
                                        keyCallback(currentKey)
                                    end
                                end
                            end)
                            return BindBtn
                        end

                        -- Attachment: Inline Color Picker Swatch
                        function ToggleObj:AddColorPicker(cpConfig)
                            cpConfig = cpConfig or {}
                            local color = cpConfig.Default or Color3.fromRGB(255, 255, 255)
                            local cpCallback = cpConfig.Callback or function() end

                            local Swatch = Instance.new("TextButton")
                            Swatch.Name = "ColorSwatch"
                            Swatch.Size = UDim2.new(0, 22, 0, 12)
                            Swatch.BackgroundColor3 = color
                            Swatch.Text = ""
                            Swatch.AutoButtonColor = false
                            Swatch.Parent = Attachments

                            local sCorner = Instance.new("UICorner")
                            sCorner.CornerRadius = UDim.new(0, 3)
                            sCorner.Parent = Swatch

                            local sStroke = Instance.new("UIStroke")
                            sStroke.Color = theme.Border
                            sStroke.Thickness = 1
                            sStroke.Parent = Swatch

                            -- Floating Color Picker Palette
                            local Palette = Instance.new("Frame")
                            Palette.Name = "PalettePopup"
                            Palette.Size = UDim2.new(0, 140, 0, 110)
                            Palette.BackgroundColor3 = theme.CardBg
                            Palette.BorderSizePixel = 0
                            Palette.ZIndex = 80
                            Palette.Visible = false
                            Palette.Parent = ScreenGui

                            local palCorner = Instance.new("UICorner")
                            palCorner.CornerRadius = UDim.new(0, 6)
                            palCorner.Parent = Palette

                            local palStroke = Instance.new("UIStroke")
                            palStroke.Color = theme.BorderLight
                            palStroke.Thickness = 1
                            palStroke.Parent = Palette

                            -- Preset Color Grid
                            local palGrid = Instance.new("UIGridLayout")
                            palGrid.CellSize = UDim2.new(0, 22, 0, 22)
                            palGrid.CellPadding = UDim2.new(0, 4, 0, 4)
                            palGrid.Parent = Palette

                            local palPad = Instance.new("UIPadding")
                            palPad.PaddingTop = UDim.new(0, 8)
                            palPad.PaddingLeft = UDim.new(0, 8)
                            palPad.Parent = Palette

                            local presetColors = {
                                Color3.fromRGB(255, 255, 255),
                                Color3.fromRGB(235, 45, 58),
                                Color3.fromRGB(50, 215, 75),
                                Color3.fromRGB(50, 130, 255),
                                Color3.fromRGB(255, 200, 40),
                                Color3.fromRGB(160, 60, 240),
                                Color3.fromRGB(255, 120, 30),
                                Color3.fromRGB(120, 120, 130),
                                Color3.fromRGB(0, 0, 0),
                                Color3.fromRGB(60, 220, 220)
                            }

                            for _, c in ipairs(presetColors) do
                                local pBtn = Instance.new("TextButton")
                                pBtn.BackgroundColor3 = c
                                pBtn.Text = ""
                                pBtn.ZIndex = 81
                                pBtn.Parent = Palette
                                local cCorn = Instance.new("UICorner")
                                cCorn.CornerRadius = UDim.new(0, 4)
                                cCorn.Parent = pBtn

                                pBtn.MouseButton1Click:Connect(function()
                                    color = c
                                    Swatch.BackgroundColor3 = c
                                    Palette.Visible = false
                                    Window.ActivePopups[Palette] = nil
                                    cpCallback(c)
                                end)
                            end

                            Swatch.MouseButton1Click:Connect(function()
                                Palette.Visible = not Palette.Visible
                                if Palette.Visible then
                                    local pos = Swatch.AbsolutePosition
                                    Palette.Position = UDim2.new(0, pos.X - 145, 0, pos.Y)
                                    Window.ActivePopups[Palette] = function()
                                        Palette.Visible = false
                                    end
                                else
                                    Window.ActivePopups[Palette] = nil
                                end
                            end)
                            return Swatch
                        end

                        -- Attachment: Gear Icon Context Menu (as seen in Chams settings!)
                        function ToggleObj:AddGearMenu(gearTitle)
                            local GearBtn = Instance.new("TextButton")
                            GearBtn.Name = "GearMenuBtn"
                            GearBtn.Size = UDim2.new(0, 16, 0, 16)
                            GearBtn.BackgroundTransparency = 1
                            GearBtn.Text = "⚙"
                            GearBtn.Font = Enum.Font.GothamMedium
                            GearBtn.TextSize = 13
                            GearBtn.TextColor3 = theme.TextDim
                            GearBtn.AutoButtonColor = false
                            GearBtn.Parent = Attachments

                            local menu = createFloatingContextMenu(GearBtn, gearTitle or toggleTitle)

                            GearBtn.MouseButton1Click:Connect(function()
                                menu:Toggle()
                            end)

                            GearBtn.MouseEnter:Connect(function()
                                tween(GearBtn, TweenInfo.new(0.15), {TextColor3 = theme.Text})
                            end)
                            GearBtn.MouseLeave:Connect(function()
                                tween(GearBtn, TweenInfo.new(0.15), {TextColor3 = theme.TextDim})
                            end)

                            return menu
                        end

                        return ToggleObj
                    end

                    -- Component: Slider (Max Distance, Scale, etc.)
                    function Card:AddSlider(sliderConfig)
                        sliderConfig = sliderConfig or {}
                        local sTitle = sliderConfig.Title or "Slider"
                        local min = sliderConfig.Min or 0
                        local max = sliderConfig.Max or 100
                        local default = sliderConfig.Default or min
                        local decimals = sliderConfig.Decimals or 0
                        local suffix = sliderConfig.Suffix or ""
                        local callback = sliderConfig.Callback or function() end

                        local value = default

                        local SliderFrame = Instance.new("Frame")
                        SliderFrame.Name = "Slider_" .. sTitle
                        SliderFrame.Size = UDim2.new(1, 0, 0, 36)
                        SliderFrame.BackgroundTransparency = 1
                        SliderFrame.Parent = ElementsContainer

                        local HeaderRow = Instance.new("Frame")
                        HeaderRow.Size = UDim2.new(1, 0, 0, 14)
                        HeaderRow.BackgroundTransparency = 1
                        HeaderRow.Parent = SliderFrame

                        local TitleLbl = Instance.new("TextLabel")
                        TitleLbl.Size = UDim2.new(1, -50, 1, 0)
                        TitleLbl.BackgroundTransparency = 1
                        TitleLbl.Font = Enum.Font.GothamMedium
                        TitleLbl.TextSize = 12
                        TitleLbl.TextColor3 = theme.Text
                        TitleLbl.TextXAlignment = Enum.TextXAlignment.Left
                        TitleLbl.Text = sTitle
                        TitleLbl.Parent = HeaderRow

                        local ValLbl = Instance.new("TextLabel")
                        ValLbl.Size = UDim2.new(0, 50, 1, 0)
                        ValLbl.Position = UDim2.new(1, -50, 0, 0)
                        ValLbl.BackgroundTransparency = 1
                        ValLbl.Font = Enum.Font.Gotham
                        ValLbl.TextSize = 11
                        ValLbl.TextColor3 = theme.TextDim
                        ValLbl.TextXAlignment = Enum.TextXAlignment.Right
                        ValLbl.Text = tostring(value) .. suffix
                        ValLbl.Parent = HeaderRow

                        -- Track
                        local Track = Instance.new("Frame")
                        Track.Name = "Track"
                        Track.Size = UDim2.new(1, 0, 0, 6)
                        Track.Position = UDim2.new(0, 0, 0, 22)
                        Track.BackgroundColor3 = theme.ElementBg
                        Track.BorderSizePixel = 0
                        Track.Parent = SliderFrame

                        local tCorner = Instance.new("UICorner")
                        tCorner.CornerRadius = UDim.new(1, 0)
                        tCorner.Parent = Track

                        local Fill = Instance.new("Frame")
                        Fill.Name = "Fill"
                        Fill.Size = UDim2.new(math.clamp((value - min) / (max - min), 0, 1), 0, 1, 0)
                        Fill.BackgroundColor3 = theme.Accent
                        Fill.BorderSizePixel = 0
                        Fill.Parent = Track

                        local fCorner = Instance.new("UICorner")
                        fCorner.CornerRadius = UDim.new(1, 0)
                        fCorner.Parent = Fill

                        local Thumb = Instance.new("Frame")
                        Thumb.Name = "Thumb"
                        Thumb.Size = UDim2.new(0, 8, 0, 12)
                        Thumb.Position = UDim2.new(1, -4, 0.5, -6)
                        Thumb.BackgroundColor3 = Color3.fromRGB(255, 255, 255)
                        Thumb.BorderSizePixel = 0
                        Thumb.Parent = Fill

                        local thCorner = Instance.new("UICorner")
                        thCorner.CornerRadius = UDim.new(0, 3)
                        thCorner.Parent = Thumb

                        local sliding = false

                        local function updateSlider(input)
                            local trackAbsPos = Track.AbsolutePosition.X
                            local trackAbsSize = Track.AbsoluteSize.X
                            local rel = math.clamp((input.Position.X - trackAbsPos) / trackAbsSize, 0, 1)
                            local rawVal = min + (max - min) * rel
                            local formatted = tonumber(string.format("%." .. decimals .. "f", rawVal))
                            value = formatted

                            Fill.Size = UDim2.new(rel, 0, 1, 0)
                            ValLbl.Text = tostring(value) .. suffix
                            callback(value)
                        end

                        Track.InputBegan:Connect(function(input)
                            if input.UserInputType == Enum.UserInputType.MouseButton1 or input.UserInputType == Enum.UserInputType.Touch then
                                sliding = true
                                updateSlider(input)
                            end
                        end)

                        UserInputService.InputEnded:Connect(function(input)
                            if input.UserInputType == Enum.UserInputType.MouseButton1 or input.UserInputType == Enum.UserInputType.Touch then
                                sliding = false
                            end
                        end)

                        UserInputService.InputChanged:Connect(function(input)
                            if sliding and (input.UserInputType == Enum.UserInputType.MouseMovement or input.UserInputType == Enum.UserInputType.Touch) then
                                updateSlider(input)
                            end
                        end)

                        local SliderObj = {}
                        function SliderObj:Set(newVal)
                            newVal = math.clamp(newVal, min, max)
                            value = tonumber(string.format("%." .. decimals .. "f", newVal))
                            local rel = (value - min) / (max - min)
                            Fill.Size = UDim2.new(rel, 0, 1, 0)
                            ValLbl.Text = tostring(value) .. suffix
                            callback(value)
                        end

                        return SliderObj
                    end

                    -- Component: Dropdown (Box, Font, Ignore, etc.)
                    function Card:AddDropdown(ddConfig)
                        ddConfig = ddConfig or {}
                        local ddTitle = ddConfig.Title or "Dropdown"
                        local options = ddConfig.Options or {}
                        local current = ddConfig.Default or options[1] or ""
                        local callback = ddConfig.Callback or function() end

                        local DropdownFrame = Instance.new("Frame")
                        DropdownFrame.Name = "Dropdown_" .. ddTitle
                        DropdownFrame.Size = UDim2.new(1, 0, 0, 44)
                        DropdownFrame.BackgroundTransparency = 1
                        DropdownFrame.Parent = ElementsContainer

                        local dLabel = Instance.new("TextLabel")
                        dLabel.Size = UDim2.new(1, 0, 0, 14)
                        dLabel.BackgroundTransparency = 1
                        dLabel.Font = Enum.Font.GothamMedium
                        dLabel.TextSize = 11
                        dLabel.TextColor3 = theme.TextDim
                        dLabel.TextXAlignment = Enum.TextXAlignment.Left
                        dLabel.Text = ddTitle
                        dLabel.Parent = DropdownFrame

                        local SelectBtn = Instance.new("TextButton")
                        SelectBtn.Size = UDim2.new(1, 0, 0, 24)
                        SelectBtn.Position = UDim2.new(0, 0, 0, 18)
                        SelectBtn.BackgroundColor3 = theme.ElementBg
                        SelectBtn.Text = ""
                        SelectBtn.AutoButtonColor = false
                        SelectBtn.Parent = DropdownFrame

                        local sbCorner = Instance.new("UICorner")
                        sbCorner.CornerRadius = UDim.new(0, 4)
                        sbCorner.Parent = SelectBtn

                        local sbStroke = Instance.new("UIStroke")
                        sbStroke.Color = theme.Border
                        sbStroke.Thickness = 1
                        sbStroke.Parent = SelectBtn

                        local ValueText = Instance.new("TextLabel")
                        ValueText.Size = UDim2.new(1, -28, 1, 0)
                        ValueText.Position = UDim2.new(0, 8, 0, 0)
                        ValueText.BackgroundTransparency = 1
                        ValueText.Font = Enum.Font.Gotham
                        ValueText.TextSize = 11
                        ValueText.TextColor3 = theme.Text
                        ValueText.TextXAlignment = Enum.TextXAlignment.Left
                        ValueText.Text = current
                        ValueText.Parent = SelectBtn

                        local Arrow = Instance.new("TextLabel")
                        Arrow.Size = UDim2.new(0, 20, 1, 0)
                        Arrow.Position = UDim2.new(1, -22, 0, 0)
                        Arrow.BackgroundTransparency = 1
                        Arrow.Font = Enum.Font.GothamBold
                        Arrow.TextSize = 9
                        Arrow.TextColor3 = theme.TextDim
                        Arrow.Text = "▼"
                        Arrow.Parent = SelectBtn

                        -- Dropdown Menu (ZIndex managed)
                        local DropList = Instance.new("Frame")
                        DropList.Name = "DropList"
                        DropList.Size = UDim2.new(1, 0, 0, 0)
                        DropList.BackgroundColor3 = theme.CardBg
                        DropList.BorderSizePixel = 0
                        DropList.ZIndex = 50
                        DropList.Visible = false
                        DropList.Parent = ScreenGui

                        local dlCorner = Instance.new("UICorner")
                        dlCorner.CornerRadius = UDim.new(0, 4)
                        dlCorner.Parent = DropList

                        local dlStroke = Instance.new("UIStroke")
                        dlStroke.Color = theme.BorderLight
                        dlStroke.Thickness = 1
                        dlStroke.Parent = DropList

                        local dlLayout = Instance.new("UIListLayout")
                        dlLayout.FillDirection = Enum.FillDirection.Vertical
                        dlLayout.Parent = DropList

                        local function updatePosition()
                            local pos = SelectBtn.AbsolutePosition
                            local size = SelectBtn.AbsoluteSize
                            DropList.Position = UDim2.new(0, pos.X, 0, pos.Y + size.Y + 4)
                            DropList.Size = UDim2.new(0, size.X, 0, #options * 22)
                        end

                        for _, opt in ipairs(options) do
                            local optBtn = Instance.new("TextButton")
                            optBtn.Size = UDim2.new(1, 0, 0, 22)
                            optBtn.BackgroundTransparency = 1
                            optBtn.Text = opt
                            optBtn.Font = Enum.Font.Gotham
                            optBtn.TextSize = 11
                            optBtn.TextColor3 = (opt == current) and theme.Accent or theme.TextDim
                            optBtn.ZIndex = 51
                            optBtn.Parent = DropList

                            optBtn.MouseButton1Click:Connect(function()
                                current = opt
                                ValueText.Text = opt
                                DropList.Visible = false
                                Window.ActivePopups[DropList] = nil
                                callback(opt)
                            end)

                            optBtn.MouseEnter:Connect(function()
                                tween(optBtn, TweenInfo.new(0.1), {TextColor3 = theme.Text})
                            end)
                            optBtn.MouseLeave:Connect(function()
                                tween(optBtn, TweenInfo.new(0.1), {
                                    TextColor3 = (opt == current) and theme.Accent or theme.TextDim
                                })
                            end)
                        end

                        SelectBtn.MouseButton1Click:Connect(function()
                            DropList.Visible = not DropList.Visible
                            if DropList.Visible then
                                updatePosition()
                                Window.ActivePopups[DropList] = function()
                                    DropList.Visible = false
                                end
                            else
                                Window.ActivePopups[DropList] = nil
                            end
                        end)

                        local DropObj = {}
                        function DropObj:Set(newOpt)
                            current = newOpt
                            ValueText.Text = newOpt
                            callback(newOpt)
                        end
                        return DropObj
                    end

                    -- Component: Button
                    function Card:AddButton(btnConfig)
                        btnConfig = btnConfig or {}
                        local bText = btnConfig.Title or "Button"
                        local bCallback = btnConfig.Callback or function() end

                        local Btn = Instance.new("TextButton")
                        Btn.Size = UDim2.new(1, 0, 0, 26)
                        Btn.BackgroundColor3 = theme.ElementBg
                        Btn.Text = bText
                        Btn.Font = Enum.Font.GothamMedium
                        Btn.TextSize = 12
                        Btn.TextColor3 = theme.Text
                        Btn.AutoButtonColor = false
                        Btn.Parent = ElementsContainer

                        local bCorner = Instance.new("UICorner")
                        bCorner.CornerRadius = UDim.new(0, 4)
                        bCorner.Parent = Btn

                        local bStroke = Instance.new("UIStroke")
                        bStroke.Color = theme.Border
                        bStroke.Thickness = 1
                        bStroke.Parent = Btn

                        Btn.MouseEnter:Connect(function()
                            tween(Btn, TweenInfo.new(0.15), {BackgroundColor3 = theme.BorderLight})
                        end)
                        Btn.MouseLeave:Connect(function()
                            tween(Btn, TweenInfo.new(0.15), {BackgroundColor3 = theme.ElementBg})
                        end)
                        Btn.MouseButton1Click:Connect(function()
                            tween(Btn, TweenInfo.new(0.1), {BackgroundColor3 = theme.AccentDim})
                            task.wait(0.1)
                            tween(Btn, TweenInfo.new(0.15), {BackgroundColor3 = theme.ElementBg})
                            bCallback()
                        end)
                        return Btn
                    end

                    -- Component: Color Row (like Highlighting: Aimbot Target, Friend, Occluded)
                    function Card:AddColorRow(colorConfig)
                        colorConfig = colorConfig or {}
                        local title = colorConfig.Title or "Color"
                        local default = colorConfig.Default or Color3.fromRGB(255, 255, 255)
                        local callback = colorConfig.Callback or function() end

                        local Row = Instance.new("Frame")
                        Row.Name = "ColorRow_" .. title
                        Row.Size = UDim2.new(1, 0, 0, 20)
                        Row.BackgroundTransparency = 1
                        Row.Parent = ElementsContainer

                        local Label = Instance.new("TextLabel")
                        Label.Size = UDim2.new(1, -30, 1, 0)
                        Label.BackgroundTransparency = 1
                        Label.Font = Enum.Font.GothamMedium
                        Label.TextSize = 12
                        Label.TextColor3 = theme.Text
                        Label.TextXAlignment = Enum.TextXAlignment.Left
                        Label.Text = title
                        Label.Parent = Row

                        local Swatch = Instance.new("TextButton")
                        Swatch.Size = UDim2.new(0, 24, 0, 14)
                        Swatch.Position = UDim2.new(1, -24, 0.5, -7)
                        Swatch.BackgroundColor3 = default
                        Swatch.Text = ""
                        Swatch.AutoButtonColor = false
                        Swatch.Parent = Row

                        local sCorner = Instance.new("UICorner")
                        sCorner.CornerRadius = UDim.new(0, 3)
                        sCorner.Parent = Swatch

                        local sStroke = Instance.new("UIStroke")
                        sStroke.Color = theme.Border
                        sStroke.Thickness = 1
                        sStroke.Parent = Swatch

                        -- Preset popup
                        local Palette = Instance.new("Frame")
                        Palette.Size = UDim2.new(0, 140, 0, 80)
                        Palette.BackgroundColor3 = theme.CardBg
                        Palette.BorderSizePixel = 0
                        Palette.ZIndex = 80
                        Palette.Visible = false
                        Palette.Parent = ScreenGui

                        local palCorner = Instance.new("UICorner")
                        palCorner.CornerRadius = UDim.new(0, 6)
                        palCorner.Parent = Palette

                        local palStroke = Instance.new("UIStroke")
                        palStroke.Color = theme.BorderLight
                        palStroke.Thickness = 1
                        palStroke.Parent = Palette

                        local palGrid = Instance.new("UIGridLayout")
                        palGrid.CellSize = UDim2.new(0, 22, 0, 22)
                        palGrid.CellPadding = UDim2.new(0, 4, 0, 4)
                        palGrid.Parent = Palette

                        local palPad = Instance.new("UIPadding")
                        palPad.PaddingTop = UDim.new(0, 8)
                        palPad.PaddingLeft = UDim.new(0, 8)
                        palPad.Parent = Palette

                        local presetColors = {
                            Color3.fromRGB(235, 45, 58),
                            Color3.fromRGB(50, 215, 75),
                            Color3.fromRGB(120, 120, 130),
                            Color3.fromRGB(50, 130, 255),
                            Color3.fromRGB(255, 200, 40),
                            Color3.fromRGB(255, 255, 255)
                        }

                        for _, c in ipairs(presetColors) do
                            local pBtn = Instance.new("TextButton")
                            pBtn.BackgroundColor3 = c
                            pBtn.Text = ""
                            pBtn.ZIndex = 81
                            pBtn.Parent = Palette
                            local cCorn = Instance.new("UICorner")
                            cCorn.CornerRadius = UDim.new(0, 4)
                            cCorn.Parent = pBtn

                            pBtn.MouseButton1Click:Connect(function()
                                Swatch.BackgroundColor3 = c
                                Palette.Visible = false
                                Window.ActivePopups[Palette] = nil
                                callback(c)
                            end)
                        end

                        Swatch.MouseButton1Click:Connect(function()
                            Palette.Visible = not Palette.Visible
                            if Palette.Visible then
                                local pos = Swatch.AbsolutePosition
                                Palette.Position = UDim2.new(0, pos.X - 145, 0, pos.Y)
                                Window.ActivePopups[Palette] = function()
                                    Palette.Visible = false
                                end
                            else
                                Window.ActivePopups[Palette] = nil
                            end
                        end)

                        return Row
                    end

                    return Card
                end

                return Column
            end

            return SubTab
        end

        table.insert(Window.Categories, Category)

        -- If first category, select it automatically
        if #Window.Categories == 1 then
            Category:Select()
        end

        return Category
    end

    return Window
end

return Photon
