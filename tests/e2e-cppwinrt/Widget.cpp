#include "winrt/Windows.Foundation.h"
#include "Widget.h"
#include <cmath>
#include <cstdlib>

namespace winrt::TestCppWinRT::implementation
{
    Widget::Widget(hstring const& name) : m_name(name) {}

    // Properties
    hstring Widget::Name() const { return m_name; }
    void Widget::Name(hstring const& value)
    {
        m_name = value;
        m_nameChanged(*this, value);
    }

    TestCppWinRT::Priority Widget::Level() const { return m_level; }
    void Widget::Level(TestCppWinRT::Priority value) { m_level = value; }

    bool Widget::IsActive() const { return m_isActive; }
    void Widget::IsActive(bool value) { m_isActive = value; }

    int32_t Widget::Count() const { return m_count; }
    void Widget::Count(int32_t value) { m_count = value; }

    // Methods
    TestCppWinRT::Point Widget::GetPosition() const { return m_position; }
    void Widget::MoveTo(TestCppWinRT::Point const& pos) { m_position = pos; }

    TestCppWinRT::Color Widget::GetColor() const
    {
        return { 255, 128, 64, 255 };
    }

    double Widget::ComputeDistance(TestCppWinRT::Point const& from, TestCppWinRT::Point const& to) const
    {
        double dx = static_cast<double>(to.X) - static_cast<double>(from.X);
        double dy = static_cast<double>(to.Y) - static_cast<double>(from.Y);
        return std::sqrt(dx * dx + dy * dy);
    }

    bool Widget::TryParse(hstring const& input, int32_t& value) const
    {
        try {
            value = std::stoi(std::wstring(input));
            return true;
        } catch (...) {
            value = 0;
            return false;
        }
    }

    // Statics
    TestCppWinRT::Widget Widget::CreateDefault()
    {
        auto w = winrt::make<Widget>(L"Default");
        w.IsActive(true);
        return w;
    }

    hstring Widget::GetVersion()
    {
        return L"1.0.0";
    }

    // Delegates
    void Widget::Subscribe(TestCppWinRT::StatusHandler const& handler)
    {
        handler(L"subscribed");
    }

    // Events
    winrt::event_token Widget::NameChanged(Windows::Foundation::TypedEventHandler<TestCppWinRT::Widget, hstring> const& handler)
    {
        return m_nameChanged.add(handler);
    }

    void Widget::NameChanged(winrt::event_token token)
    {
        m_nameChanged.remove(token);
    }
}
