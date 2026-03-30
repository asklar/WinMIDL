#include "winrt/Windows.Foundation.h"
#include "Widget.h"
#include <cmath>

namespace winrt::TestCppWinRT::implementation
{
    Widget::Widget(hstring const& name) : m_name(name) {}

    hstring Widget::Name() const { return m_name; }
    void Widget::Name(hstring const& value)
    {
        m_name = value;
        m_nameChanged(*this, value);
    }

    TestCppWinRT::Priority Widget::Level() const { return m_level; }
    void Widget::Level(TestCppWinRT::Priority value) { m_level = value; }

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

    TestCppWinRT::Widget Widget::CreateDefault()
    {
        return winrt::make<Widget>(L"Default");
    }

    hstring Widget::GetVersion()
    {
        return L"1.0.0";
    }

    void Widget::Subscribe(TestCppWinRT::StatusHandler const& handler)
    {
        handler(L"subscribed");
    }

    winrt::event_token Widget::NameChanged(Windows::Foundation::TypedEventHandler<TestCppWinRT::Widget, hstring> const& handler)
    {
        return m_nameChanged.add(handler);
    }

    void Widget::NameChanged(winrt::event_token token)
    {
        m_nameChanged.remove(token);
    }
}
